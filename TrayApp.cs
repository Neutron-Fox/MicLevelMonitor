using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;

namespace MicLevelMonitor
{
    public class TrayApp : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Timer updateTimer;
        private WaveInEvent waveIn;
        private float currentLevel = 0;
        private const int IconSize = 32;

        public TrayApp()
        {
            notifyIcon = new NotifyIcon
            {
                Text = "Mikrofon Monitor - Inicializálás...",
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Kilépés", null, (s, e) => Exit());
            notifyIcon.ContextMenuStrip = contextMenu;

            InitializeMicrophone();

            updateTimer = new Timer { Interval = 100 };
            updateTimer.Tick += UpdateIcon;
            updateTimer.Start();

            UpdateIcon(null, null);
        }

        private void InitializeMicrophone()
        {
            try
            {
                if (WaveIn.DeviceCount == 0)
                {
                    notifyIcon.Text = "Nincs mikrofon";
                    return;
                }

                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = 0;
                waveIn.WaveFormat = new WaveFormat(44100, 1);
                waveIn.BufferMilliseconds = 50;
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.StartRecording();

                var deviceCaps = WaveIn.GetCapabilities(0);
                notifyIcon.Text = $"Mikrofon: {deviceCaps.ProductName}";
            }
            catch (Exception ex)
            {
                notifyIcon.Text = $"Hiba: {ex.Message}";
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            float sum = 0;
            int sampleCount = e.BytesRecorded / 2;

            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                float sampleValue = sample / 32768f;
                sum += sampleValue * sampleValue;
            }

            if (sampleCount > 0)
            {
                float rms = (float)Math.Sqrt(sum / sampleCount);
                // Érzékenyebb skála - logaritmikus skálázás és erősítés
                currentLevel = (float)Math.Min(1.0, Math.Log10(rms * 100 + 1) / 2.0);
            }
        }

        private void UpdateIcon(object sender, EventArgs e)
        {
            float level = currentLevel;
            bool hasValidLevel = waveIn != null;

            if (hasValidLevel)
            {
                notifyIcon.Text = $"Mikrofon: {level * 100:F1}%";
            }
            else
            {
                notifyIcon.Text = "Mikrofon: Nincs kapcsolat";
            }

            GenerateIcon(level, hasValidLevel);
        }

        private void GenerateIcon(float level, bool hasValidLevel)
        {
            using var bmp = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bmp);

            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color frameColor = hasValidLevel ? Color.Gray : Color.Red;
            using var pen = new Pen(frameColor, 2);
            g.DrawRectangle(pen, 1, 1, IconSize - 3, IconSize - 3);

            if (!hasValidLevel)
            {
                g.DrawLine(pen, 4, 4, IconSize - 4, IconSize - 4);
                g.DrawLine(pen, IconSize - 4, 4, 4, IconSize - 4);
            }
            else
            {
                // 8 sávos megjelenítés érzékenyebb skálával
                int bars = Math.Min(8, (int)(level * 12)); // Érzékenyebb skála

                const int barCount = 8;
                const int spacing = 1;
                const int margin = 3;

                int availableHeight = IconSize - 2 * margin;
                int barHeight = (availableHeight - (barCount - 1) * spacing) / barCount;
                int barWidth = IconSize - 2 * margin;
                int x = margin;

                for (int i = 0; i < barCount; i++)
                {
                    int y = IconSize - margin - (i + 1) * (barHeight + spacing) + spacing;

                    Color color;
                    if (i < bars)
                    {
                        // Színátmenet: 0-2 zöld, 3-5 sárga, 6-7 piros
                        if (i <= 2) color = Color.Lime;
                        else if (i <= 5) color = Color.Gold;
                        else color = Color.Red;
                    }
                    else
                    {
                        color = Color.FromArgb(50, Color.LightGray);
                    }

                    using var brush = new SolidBrush(color);
                    g.FillRectangle(brush, x, y, barWidth, barHeight);
                }
            }

            IntPtr hIcon = bmp.GetHicon();
            try
            {
                notifyIcon.Icon = Icon.FromHandle(hIcon);
            }
            finally
            {
                DestroyIcon(hIcon);
            }
        }

        private void Exit()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            updateTimer?.Stop();
            updateTimer?.Dispose();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}