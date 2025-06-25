using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.Wave;

namespace MicLevelMonitor
{
    public sealed class TrayApp : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Timer updateTimer;
        private WaveInEvent waveIn;
        private float currentLevel;

        // Konstansok
        private const int IconSize = 32;
        private const int BarCount = 8;
        private const int UpdateInterval = 100; // ms

        // Statikus színek - nem kell minden alkalommal újra létrehozni
        private static readonly Color[] BarColors = { Color.Lime, Color.Lime, Color.Lime, Color.Gold, Color.Gold, Color.Gold, Color.Red, Color.Red };
        private static readonly Color InactiveBarColor = Color.FromArgb(50, Color.LightGray);
        private static readonly Color FrameColor = Color.Gray;
        private static readonly Color ErrorColor = Color.Red;

        // Egyetlen icon instance
        private Icon currentIcon;

        public TrayApp()
        {
            // Tray icon inicializálás
            notifyIcon = new NotifyIcon
            {
                Text = "Mikrofon Monitor",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            // Mikrofon inicializálás
            InitializeMicrophone();

            // Timer inicializálás - csak egy timer
            updateTimer = new Timer { Interval = UpdateInterval };
            updateTimer.Tick += UpdateIcon;
            updateTimer.Start();

            // Első icon generálás
            UpdateIcon(null, null);
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Kilépés", null, Exit);
            return contextMenu;
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

                waveIn = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(44100, 1),
                    BufferMilliseconds = 50
                };

                waveIn.DataAvailable += OnDataAvailable;
                waveIn.StartRecording();

                // Eszköz név lekérése
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
            // Gyors RMS számítás
            float sum = 0;
            int sampleCount = e.BytesRecorded / 2;

            unsafe
            {
                fixed (byte* buffer = e.Buffer)
                {
                    short* samples = (short*)buffer;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        float sample = samples[i] / 32768f;
                        sum += sample * sample;
                    }
                }
            }

            if (sampleCount > 0)
            {
                float rms = (float)Math.Sqrt(sum / sampleCount);
                currentLevel = Math.Min(1.0f, (float)(Math.Log10(rms * 100 + 1) / 2.0));
            }
        }

        private void UpdateIcon(object sender, EventArgs e)
        {
            bool hasValidLevel = waveIn != null;

            // Tooltip frissítés
            notifyIcon.Text = hasValidLevel
                ? $"Mikrofon: {currentLevel * 100:F1}%"
                : "Mikrofon: Nincs kapcsolat";

            // Icon generálás
            GenerateIcon(currentLevel, hasValidLevel);
        }

        private void GenerateIcon(float level, bool hasValidLevel)
        {
            // Előző icon dispose
            currentIcon?.Dispose();

            using (var bmp = new Bitmap(IconSize, IconSize))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None; // Gyorsabb

                // Keret rajzolás
                Color frameColor = hasValidLevel ? FrameColor : ErrorColor;
                using (var pen = new Pen(frameColor, 2))
                {
                    g.DrawRectangle(pen, 1, 1, IconSize - 3, IconSize - 3);

                    if (!hasValidLevel)
                    {
                        // X rajzolás hiba esetén
                        g.DrawLine(pen, 4, 4, IconSize - 4, IconSize - 4);
                        g.DrawLine(pen, IconSize - 4, 4, 4, IconSize - 4);
                    }
                    else
                    {
                        DrawLevelBars(g, level);
                    }
                }

                // Icon létrehozás
                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    currentIcon = Icon.FromHandle(hIcon);
                    notifyIcon.Icon = currentIcon;
                }
                finally
                {
                    // Handle destroy
                    DestroyIcon(hIcon);
                }
            }
        }

        private void DrawLevelBars(Graphics g, float level)
        {
            int activeBars = Math.Min(BarCount, (int)(level * BarCount * 1.5f));

            const int margin = 3;
            const int spacing = 1;
            int availableHeight = IconSize - 2 * margin;
            int barHeight = (availableHeight - (BarCount - 1) * spacing) / BarCount;
            int barWidth = IconSize - 2 * margin;

            for (int i = 0; i < BarCount; i++)
            {
                int y = IconSize - margin - (i + 1) * (barHeight + spacing) + spacing;
                Color color = i < activeBars ? BarColors[i] : InactiveBarColor;

                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, margin, y, barWidth, barHeight);
                }
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            // Cleanup
            updateTimer?.Stop();
            updateTimer?.Dispose();

            waveIn?.StopRecording();
            waveIn?.Dispose();

            currentIcon?.Dispose();

            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            Application.Exit();
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}