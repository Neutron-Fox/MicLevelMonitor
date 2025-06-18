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
            Console.WriteLine("TrayApp inicializálása...");

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

            Console.WriteLine("TrayApp inicializálása kész.");
            UpdateIcon(null, null);
        }

        private void InitializeMicrophone()
        {
            Console.WriteLine("Mikrofon inicializálása (WaveIn)...");

            try
            {
                // Elérhető eszközök listázása
                Console.WriteLine($"Elérhető mikrofon eszközök: {WaveIn.DeviceCount}");
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var caps = WaveIn.GetCapabilities(i);
                    Console.WriteLine($"  {i}: {caps.ProductName}");
                }

                if (WaveIn.DeviceCount == 0)
                {
                    Console.WriteLine("Nincs elérhető mikrofon eszköz!");
                    notifyIcon.Text = "Nincs mikrofon";
                    return;
                }

                // WaveIn beállítása
                waveIn = new WaveInEvent();
                waveIn.DeviceNumber = 0; // Első mikrofon
                waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, mono
                waveIn.BufferMilliseconds = 50; // 50ms buffer

                // Eseménykezelő a hangadatok fogadására
                waveIn.DataAvailable += OnDataAvailable;

                Console.WriteLine("WaveIn beállítva. Felvétel indítása...");
                waveIn.StartRecording();

                var deviceCaps = WaveIn.GetCapabilities(0);
                notifyIcon.Text = $"Mikrofon: {deviceCaps.ProductName}";
                Console.WriteLine($"Felvétel elindítva: {deviceCaps.ProductName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mikrofon inicializálási hiba: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                notifyIcon.Text = $"Hiba: {ex.Message}";
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            // Hangerő számítása a bejövő adatokból
            float sum = 0;
            int sampleCount = e.BytesRecorded / 2; // 16-bit minták

            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                float sampleValue = sample / 32768f; // Normalizálás -1 és 1 közé
                sum += sampleValue * sampleValue; // RMS számítás
            }

            if (sampleCount > 0)
            {
                float rms = (float)Math.Sqrt(sum / sampleCount);
                currentLevel = rms;

                // Debug kiírás (ritkábban)
                if (rms > 0.001f) // Csak ha van jelentős hang
                {
                    Console.WriteLine($"Hangerő: {rms:F4} ({rms * 100:F1}%)");
                }
            }
        }

        private void UpdateIcon(object sender, EventArgs e)
        {
            float level = currentLevel;
            bool hasValidLevel = waveIn != null;

            // Tooltip frissítése
            if (hasValidLevel)
            {
                notifyIcon.Text = $"Mikrofon: {level * 100:F1}%";
            }
            else
            {
                notifyIcon.Text = "Mikrofon: Nincs kapcsolat";
            }

            // Ikon generálása
            GenerateIcon(level, hasValidLevel);
        }

        private void GenerateIcon(float level, bool hasValidLevel)
        {
            using var bmp = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bmp);

            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Négyzetes keret - piros ha nincs adat, szürke ha van
            Color frameColor = hasValidLevel ? Color.Gray : Color.Red;
            using var pen = new Pen(frameColor, 2);
            g.DrawRectangle(pen, 1, 1, IconSize - 3, IconSize - 3);

            if (!hasValidLevel)
            {
                // X rajzolása ha nincs mikrofon
                g.DrawLine(pen, 4, 4, IconSize - 4, IconSize - 4);
                g.DrawLine(pen, IconSize - 4, 4, 4, IconSize - 4);
            }
            else
            {
                // Sávok számítása - érzékenyebb skála
                int bars = Math.Min(7, (int)(level * 50));

                const int barCount = 7;
                const int spacing = 1;
                const int margin = 3; // Kisebb margó hogy kitöltse a négyzetet

                // Vertikális sávok számítása - kitöltik a négyzetet
                int availableHeight = IconSize - 2 * margin;
                int barHeight = (availableHeight - (barCount - 1) * spacing) / barCount;
                int barWidth = IconSize - 2 * margin; // Széles sávok
                int startY = margin;
                int x = margin;

                // Sávok rajzolása alulról felfelé
                for (int i = 0; i < barCount; i++)
                {
                    int y = IconSize - margin - (i + 1) * (barHeight + spacing) + spacing;

                    Color color;
                    if (i < bars)
                    {
                        // Aktív sáv színe - alul zöld, felül piros
                        if (i < 3) color = Color.Lime;
                        else if (i < 5) color = Color.Gold;
                        else color = Color.Red;
                    }
                    else
                    {
                        // Inaktív sáv színe
                        color = Color.FromArgb(50, Color.LightGray);
                    }

                    using var brush = new SolidBrush(color);
                    g.FillRectangle(brush, x, y, barWidth, barHeight);
                }
            }

            // Ikon beállítása
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
            Console.WriteLine("Kilépés...");

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