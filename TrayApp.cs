using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace MicLevelMonitor
{
    public sealed class TrayApp : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly WinFormsTimer updateTimer;
        private readonly WinFormsTimer reconnectTimer;
        private WaveInEvent? waveIn;
        private float currentVolumePercent;
        private bool isDisposing;
        private float volumeSmoothing = 0.90f; // Még simább változás (volt: 0.9f)
        private int selectedDeviceNumber = -1;
        private Form? deviceSelectForm;
        private readonly string settingsPath;
        private bool isDarkMode;

        private const int IconSize = 32;
        private const int BarCount = 8;
        private const int UpdateInterval = 16;
        private const int ReconnectInterval = 2000;
        private const float BaseAmplification = 100f; // Csökkentett érzékenység halk hangokra (volt: 150f)
        private const float LowVolumeBoost = 0.8f; // Még kevesebb erősítés halk hangoknál (volt: 1.0f)
        private const float MinimumThreshold = 0.015f; // Minimum küszöb a zajszűréshez

        // Téma színek
        private static readonly Color DarkBackColor = Color.Black;
        private static readonly Color DarkTextColor = Color.White;
        private static readonly Color LightBackColor = Color.White;
        private static readonly Color LightTextColor = Color.Black;

        // Ikon színek - ezek nem változnak a témával
        private static readonly Color[] BarColors = {
            Color.FromArgb(255, 0, 255, 0),     // Zöld
            Color.FromArgb(255, 150, 255, 0),    // Világoszöld
            Color.FromArgb(255, 200, 255, 0),    // Sárgászöld
            Color.FromArgb(255, 255, 255, 0),    // Sárga
            Color.FromArgb(255, 255, 200, 0),    // Narancssárga
            Color.FromArgb(255, 255, 150, 0),    // Világos narancssárga
            Color.FromArgb(255, 255, 100, 0),    // Sötét narancssárga
            Color.FromArgb(255, 255, 0, 0)       // Piros
        };
        private static readonly Color InactiveBarColor = Color.FromArgb(30, Color.LightGray);
        private static readonly Color FrameColor = Color.FromArgb(150, Color.Gray);
        private static readonly Color ErrorColor = Color.Red;

        private Icon? currentIcon;

        public TrayApp()
        {
            // Windows téma figyelése
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            UpdateTheme();

            // Beállítások fájl az exe mellé
            settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "device.settings");

            notifyIcon = new NotifyIcon
            {
                Text = "Mikrofon Monitor - Inicializálás...",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            updateTimer = new WinFormsTimer { Interval = UpdateInterval };
            updateTimer.Tick += UpdateIcon;

            reconnectTimer = new WinFormsTimer { Interval = ReconnectInterval };
            reconnectTimer.Tick += (s, e) => TryReconnectMicrophone();

            SystemEvents.SessionSwitch += OnSessionSwitch;
            
            LoadSettings();
            if (selectedDeviceNumber == -1 || !IsDeviceValid(selectedDeviceNumber))
            {
                ShowDeviceSelector(this, EventArgs.Empty);
            }
            else
            {
                InitializeMicrophone();
            }
            
            updateTimer.Start();
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip
            {
                ShowImageMargin = false, // Ez eltávolítja a bal oldali fehér négyzeteket
                ShowCheckMargin = false  // Ez is segít eltávolítani a margót
            };
            
            ApplyThemeToMenu(menu);
            
            var micItem = new ToolStripMenuItem("Mikrofon választás", null, ShowDeviceSelector)
            {
                Image = null,
                ImageScaling = ToolStripItemImageScaling.None
            };
            
            var exitItem = new ToolStripMenuItem("Kilépés", null, Exit)
            {
                Image = null,
                ImageScaling = ToolStripItemImageScaling.None
            };
            
            menu.Items.Add(micItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);
            
            return menu;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                UpdateTheme();
            }
        }

        private void UpdateTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    var value = key.GetValue("AppsUseLightTheme");
                    isDarkMode = value != null && (int)value == 0;
                }
            }
            catch
            {
                isDarkMode = false;
            }

            if (notifyIcon?.ContextMenuStrip != null)
            {
                ApplyThemeToMenu(notifyIcon.ContextMenuStrip);
            }

            if (deviceSelectForm != null && !deviceSelectForm.IsDisposed)
            {
                ApplyThemeToForm(deviceSelectForm);
            }
        }

        private void ApplyThemeToMenu(ContextMenuStrip menu)
        {
            menu.BackColor = isDarkMode ? DarkBackColor : LightBackColor;
            menu.ForeColor = isDarkMode ? DarkTextColor : LightTextColor;
            menu.ShowImageMargin = false;
            menu.ShowCheckMargin = false;
            
            foreach (ToolStripItem item in menu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.BackColor = isDarkMode ? DarkBackColor : LightBackColor;
                    menuItem.ForeColor = isDarkMode ? DarkTextColor : LightTextColor;
                    menuItem.Image = null;
                    menuItem.ImageScaling = ToolStripItemImageScaling.None;
                }
                else if (item is ToolStripSeparator separator)
                {
                    separator.BackColor = isDarkMode ? DarkBackColor : LightBackColor;
                    separator.ForeColor = isDarkMode ? Color.Gray : Color.DarkGray;
                }
            }
        }

        private void ApplyThemeToForm(Form form)
        {
            form.BackColor = isDarkMode ? DarkBackColor : LightBackColor;
            form.ForeColor = isDarkMode ? DarkTextColor : LightTextColor;

            foreach (Control control in form.Controls)
            {
                if (control is ListBox listBox)
                {
                    listBox.BackColor = isDarkMode ? DarkBackColor : LightBackColor;
                    listBox.ForeColor = isDarkMode ? DarkTextColor : LightTextColor;
                }
                else if (control is Panel panel)
                {
                    panel.BackColor = isDarkMode ? DarkBackColor : LightBackColor;
                    foreach (Control btnControl in panel.Controls)
                    {
                        if (btnControl is Button button)
                        {
                            button.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : SystemColors.Control;
                            button.ForeColor = isDarkMode ? DarkTextColor : LightTextColor;
                            button.FlatStyle = isDarkMode ? FlatStyle.Flat : FlatStyle.Standard;
                        }
                    }
                }
            }
        }

        private bool IsDeviceValid(int deviceNumber)
        {
            try
            {
                if (deviceNumber >= 0 && deviceNumber < WaveIn.DeviceCount)
                {
                    var caps = WaveIn.GetCapabilities(deviceNumber);
                    return !string.IsNullOrEmpty(caps.ProductName);
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                    if (settings.TryGetValue("deviceNumber", out int deviceNum))
                    {
                        selectedDeviceNumber = deviceNum;
                    }
                }
            }
            catch
            {
                selectedDeviceNumber = -1;
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new Dictionary<string, int> { { "deviceNumber", selectedDeviceNumber } };
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(settingsPath, json);
            }
            catch
            {
                // Hiba esetén nem kritikus, a következő indításnál újra kiválasztható az eszköz
            }
        }

        private void ShowDeviceSelector(object sender, EventArgs e)
        {
            if (deviceSelectForm?.IsDisposed == false) return;

            deviceSelectForm = new Form
            {
                Text = "Mikrofon választás",
                Size = new Size(600, 400), // Nagyobb ablak a teljes nevekhez
                FormBorderStyle = FormBorderStyle.Sizable, // Átméretezhető ablak
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterScreen,
                MinimumSize = new Size(400, 300) // Minimum méret
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                DrawMode = DrawMode.OwnerDrawVariable,
                IntegralHeight = false
            };

            // Dupla kattintás esemény hozzáadása
            listBox.MouseDoubleClick += (s, args) =>
            {
                if (listBox.SelectedItem is DeviceItem selectedItem)
                {
                    selectedDeviceNumber = selectedItem.DeviceNumber;
                    SaveSettings();
                    InitializeMicrophone();
                    deviceSelectForm.DialogResult = DialogResult.OK;
                    deviceSelectForm.Close();
                }
            };

            // Egyedi rajzolás: teljes név, sortörés, szürke vonal
            listBox.MeasureItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= listBox.Items.Count) return;
                string text = listBox.Items[e.Index].ToString();
                var size = e.Graphics.MeasureString(text, listBox.Font, listBox.Width - 20);
                e.ItemHeight = (int)Math.Ceiling(size.Height) + 12; // Több hely a szöveghez
            };
            
            listBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= listBox.Items.Count) return;
                e.DrawBackground();
                string text = listBox.Items[e.Index].ToString();
                var foreColor = isDarkMode ? DarkTextColor : LightTextColor;
                var backColor = isDarkMode ? DarkBackColor : LightBackColor;
                
                // Kiválasztott elem háttérszíne
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    backColor = isDarkMode ? Color.FromArgb(50, 50, 50) : Color.FromArgb(0, 120, 215);
                    foreColor = Color.White;
                }
                
                using var brush = new SolidBrush(foreColor);
                using var backBrush = new SolidBrush(backColor);
                
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                
                // Teljes szöveg kirajzolása sortöréssel
                var textRect = new RectangleF(e.Bounds.X + 8, e.Bounds.Y + 4, e.Bounds.Width - 16, e.Bounds.Height - 8);
                var format = new StringFormat { FormatFlags = StringFormatFlags.LineLimit };
                e.Graphics.DrawString(text, listBox.Font, brush, textRect, format);
                
                // Szürke elválasztó vonal
                if (e.Index < listBox.Items.Count - 1)
                {
                    using var pen = new Pen(isDarkMode ? Color.Gray : Color.LightGray, 1);
                    e.Graphics.DrawLine(pen, e.Bounds.Left + 4, e.Bounds.Bottom - 1, e.Bounds.Right - 4, e.Bounds.Bottom - 1);
                }
                
                e.DrawFocusRectangle();
            };

            // Elérhető eszközök betöltése
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                listBox.Items.Add(new DeviceItem(i, caps.ProductName));
            }

            if (listBox.Items.Count == 0)
            {
                listBox.Items.Add("Nem található mikrofon");
                listBox.Enabled = false;
            }
            else if (selectedDeviceNumber >= 0)
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if (listBox.Items[i] is DeviceItem item && item.DeviceNumber == selectedDeviceNumber)
                    {
                        listBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 }; // Magasabb panel
            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Enabled = listBox.Items.Count > 0 && listBox.Enabled,
                Width = 80,
                Height = 30,
                Location = new Point(buttonPanel.Width / 2 - 85, 15)
            };

            var cancelButton = new Button
            {
                Text = "Mégse",
                DialogResult = DialogResult.Cancel,
                Width = 80,
                Height = 30,
                Location = new Point(buttonPanel.Width / 2 + 5, 15)
            };

            // Gombok pozicionálása az ablak átméretezésekor
            buttonPanel.Resize += (s, args) =>
            {
                okButton.Location = new Point(buttonPanel.Width / 2 - 85, 15);
                cancelButton.Location = new Point(buttonPanel.Width / 2 + 5, 15);
            };

            buttonPanel.Controls.AddRange(new Control[] { okButton, cancelButton });
            deviceSelectForm.Controls.AddRange(new Control[] { listBox, buttonPanel });

            deviceSelectForm.AcceptButton = okButton;
            deviceSelectForm.CancelButton = cancelButton;

            ApplyThemeToForm(deviceSelectForm);

            if (deviceSelectForm.ShowDialog() == DialogResult.OK && listBox.SelectedItem is DeviceItem selectedItem)
            {
                selectedDeviceNumber = selectedItem.DeviceNumber;
                SaveSettings();
                InitializeMicrophone();
            }

            deviceSelectForm.Dispose();
            deviceSelectForm = null;
        }

        private class DeviceItem
        {
            public int DeviceNumber { get; }
            public string Name { get; }

            public DeviceItem(int number, string name)
            {
                DeviceNumber = number;
                Name = name;
            }

            public override string ToString() => Name;
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    SafeStopRecording();
                    break;
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                    if (!isDisposing) TryReconnectMicrophone();
                    break;
            }
        }

        private void SafeStopRecording()
        {
            try
            {
                if (waveIn != null)
                {
                    waveIn.StopRecording();
                    waveIn.Dispose();
                    waveIn = null;
                }
            }
            catch (Exception ex)
            {
                notifyIcon.Text = $"Hiba a mikrofon leállításánál: {ex.Message}";
            }
        }

        private void TryReconnectMicrophone()
        {
            reconnectTimer.Stop();
            if (!isDisposing)
            {
                if (!IsDeviceValid(selectedDeviceNumber))
                {
                    ShowDeviceSelector(this, EventArgs.Empty);
                }
                else
                {
                    InitializeMicrophone();
                }
            }
        }

        private void InitializeMicrophone()
        {
            try
            {
                SafeStopRecording();

                if (selectedDeviceNumber < 0 || !IsDeviceValid(selectedDeviceNumber))
                {
                    notifyIcon.Text = "Nincs kiválasztott mikrofon";
                    ShowDeviceSelector(this, EventArgs.Empty);
                    return;
                }

                waveIn = new WaveInEvent
                {
                    DeviceNumber = selectedDeviceNumber,
                    WaveFormat = new WaveFormat(44100, 1),
                    BufferMilliseconds = 10
                };

                waveIn.DataAvailable += OnDataAvailable;
                waveIn.StartRecording();

                var deviceCaps = WaveIn.GetCapabilities(selectedDeviceNumber);
                notifyIcon.Text = $"Mikrofon: {deviceCaps.ProductName}";
            }
            catch (Exception ex)
            {
                notifyIcon.Text = $"Hiba: {ex.Message}";
                reconnectTimer.Start();
            }
        }

        // Biztonságos hangerő feldolgozás - csökkentett érzékenység halk hangokra
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (isDisposing) return;

            float sum = 0;
            int sampleCount = e.BytesRecorded / 2;

            // Biztonságos audio feldolgozás
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                float sampleValue = sample / 32768f;
                sum += sampleValue * sampleValue;
            }

            if (sampleCount > 0)
            {
                float rms = (float)Math.Sqrt(sum / sampleCount);
                
                // Zajszűrés - nagyon halk hangok kiszűrése
                if (rms < MinimumThreshold)
                {
                    // Exponenciális simítás 0 felé nagyon halk hangoknál
                    currentVolumePercent = currentVolumePercent * 0.95f;
                    return;
                }
                
                float rawVolume = rms * BaseAmplification;
                
                // Csökkentett boost logika csak valóban halk, de hallható hangoknál
                if (rawVolume < 15) // Magasabb küszöb (volt: 20)
                {
                    rawVolume *= LowVolumeBoost;
                }
                
                float expVolume = (float)(Math.Pow(rawVolume, 0.8) * 100); // Kevésbé agresszív görbe (volt: 0.7)
                float newVolume = Math.Min(100, expVolume);

                // Exponenciális simítás
                currentVolumePercent = (currentVolumePercent * volumeSmoothing) + (newVolume * (1 - volumeSmoothing));
            }
        }

        private void UpdateIcon(object sender, EventArgs e)
        {
            if (isDisposing) return;

            bool hasValidLevel = waveIn != null && IsDeviceValid(selectedDeviceNumber);
            
            if (hasValidLevel)
            {
                var deviceCaps = WaveIn.GetCapabilities(selectedDeviceNumber);
                notifyIcon.Text = $"{deviceCaps.ProductName}: {(int)currentVolumePercent}%";
            }
            else
            {
                notifyIcon.Text = "Mikrofon: Nincs kapcsolat";
            }

            GenerateIcon((int)currentVolumePercent, hasValidLevel);
        }

        private void GenerateIcon(int volumePercent, bool hasValidLevel)
        {
            currentIcon?.Dispose();

            using var bmp = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bmp);
            
            // Mindig fekete háttér a jobb láthatóság érdekében
            g.Clear(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            using (var pen = new Pen(hasValidLevel ? FrameColor : ErrorColor, 2))
            {
                g.DrawRectangle(pen, 1, 1, IconSize - 3, IconSize - 3);

                if (!hasValidLevel)
                {
                    g.DrawLine(pen, 4, 4, IconSize - 4, IconSize - 4);
                    g.DrawLine(pen, IconSize - 4, 4, 4, IconSize - 4);
                }
                else
                {
                    DrawLevelBars(g, volumePercent);
                }
            }

            IntPtr hIcon = bmp.GetHicon();
            try
            {
                currentIcon = Icon.FromHandle(hIcon);
                notifyIcon.Icon = currentIcon;
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        private void DrawLevelBars(Graphics g, int volumePercent)
        {
            float sensitivity = volumePercent < 15 ? 1.4f : 1.2f; // Csökkentett érzékenység minden szinten (volt: 1.8f/1.5f)
            int activeBars = Math.Min(BarCount, (int)(volumePercent * BarCount * sensitivity / 100));

            const int margin = 3;
            const int spacing = 1;
            int availableHeight = IconSize - 2 * margin;
            int barHeight = (availableHeight - (BarCount - 1) * spacing) / BarCount;
            int barWidth = IconSize - 2 * margin;

            for (int i = 0; i < BarCount; i++)
            {
                int y = IconSize - margin - (i + 1) * (barHeight + spacing) + spacing;
                using var brush = new SolidBrush(i < activeBars ? BarColors[i] : InactiveBarColor);
                g.FillRectangle(brush, margin, y, barWidth, barHeight);
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            isDisposing = true;
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            
            updateTimer?.Stop();
            updateTimer?.Dispose();
            
            reconnectTimer?.Stop();
            reconnectTimer?.Dispose();

            SafeStopRecording();
            currentIcon?.Dispose();
            deviceSelectForm?.Close();
            deviceSelectForm?.Dispose();

            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            Application.Exit();
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}