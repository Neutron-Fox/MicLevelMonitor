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
        private readonly Timer memoryCleanupTimer;
        private WaveInEvent waveIn;
        private float currentLevel;
        private const int IconSize = 32;
        private const int BarCount = 8;

        // Memória optimalizálás
        private Icon currentIcon;
        private int updateCounter = 0;
        private const int GC_COLLECT_INTERVAL = 100; // 10 másodpercenként (100 * 100ms)

        public TrayApp()
        {
            notifyIcon = new NotifyIcon
            {
                Text = "Mikrofon Monitor",
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Kilépés", null, Exit);
            notifyIcon.ContextMenuStrip = contextMenu;

            InitializeMicrophone();

            // Fő update timer
            updateTimer = new Timer { Interval = 100 };
            updateTimer.Tick += UpdateIcon;
            updateTimer.Start();

            // Memória cleanup timer - ritkábban fut
            memoryCleanupTimer = new Timer { Interval = 5000 }; // 5 másodpercenként
            memoryCleanupTimer.Tick += (s, e) => ForceMemoryCleanup();
            memoryCleanupTimer.Start();

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

                waveIn = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(44100, 1),
                    BufferMilliseconds = 50
                };

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
                currentLevel = Math.Min(1.0f, (float)(Math.Log10(rms * 100 + 1) / 2.0));
            }
        }

        private void UpdateIcon(object sender, EventArgs e)
        {
            bool hasValidLevel = waveIn != null;

            notifyIcon.Text = hasValidLevel
                ? $"Mikrofon: {currentLevel * 100:F1}%"
                : "Mikrofon: Nincs kapcsolat";

            GenerateIcon(currentLevel, hasValidLevel);

            // Periodikus memória cleanup
            updateCounter++;
            if (updateCounter >= GC_COLLECT_INTERVAL)
            {
                updateCounter = 0;
                ForceMemoryCleanup();
            }
        }

        private void GenerateIcon(float level, bool hasValidLevel)
        {
            // Előző icon felszabadítása
            if (currentIcon != null)
            {
                currentIcon.Dispose();
                currentIcon = null;
            }

            // Bitmap és Graphics using-ban a proper dispose-ért
            using (var bmp = new Bitmap(IconSize, IconSize))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    Color frameColor = hasValidLevel ? Color.Gray : Color.Red;
                    using (var pen = new Pen(frameColor, 2))
                    {
                        g.DrawRectangle(pen, 1, 1, IconSize - 3, IconSize - 3);

                        if (!hasValidLevel)
                        {
                            g.DrawLine(pen, 4, 4, IconSize - 4, IconSize - 4);
                            g.DrawLine(pen, IconSize - 4, 4, 4, IconSize - 4);
                        }
                        else
                        {
                            DrawLevelBars(g, level);
                        }
                    }
                }

                // Icon létrehozása és beállítása
                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    currentIcon = Icon.FromHandle(hIcon);
                    notifyIcon.Icon = currentIcon;
                }
                catch
                {
                    // Ha valami hiba van, destroy-oljuk az icon handle-t
                    DestroyIcon(hIcon);
                }
                finally
                {
                    // A handle-t mindenképpen destroy-olni kell
                    if (hIcon != IntPtr.Zero)
                    {
                        DestroyIcon(hIcon);
                    }
                }
            }
        }

        private void DrawLevelBars(Graphics g, float level)
        {
            int bars = Math.Min(BarCount, (int)(level * 12));
            const int spacing = 1;
            const int margin = 3;

            int availableHeight = IconSize - 2 * margin;
            int barHeight = (availableHeight - (BarCount - 1) * spacing) / BarCount;
            int barWidth = IconSize - 2 * margin;
            int x = margin;

            for (int i = 0; i < BarCount; i++)
            {
                int y = IconSize - margin - (i + 1) * (barHeight + spacing) + spacing;

                Color color = i < bars ? GetBarColor(i) : Color.FromArgb(50, Color.LightGray);

                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x, y, barWidth, barHeight);
                }
            }
        }

        private static Color GetBarColor(int barIndex)
        {
            return barIndex switch
            {
                <= 2 => Color.Lime,
                <= 5 => Color.Gold,
                _ => Color.Red
            };
        }

        private void ForceMemoryCleanup()
        {
            // Explicit garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Memória kompakció (ritkán használandó)
            GC.Collect(2, GCCollectionMode.Forced, true, true);
        }

        private void Exit(object sender, EventArgs e)
        {
            // Összes resource felszabadítása
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveIn = null;

            updateTimer?.Stop();
            updateTimer?.Dispose();

            memoryCleanupTimer?.Stop();
            memoryCleanupTimer?.Dispose();

            // Icon felszabadítása
            if (currentIcon != null)
            {
                currentIcon.Dispose();
                currentIcon = null;
            }

            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            // Végső memória cleanup
            ForceMemoryCleanup();

            Application.Exit();
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}