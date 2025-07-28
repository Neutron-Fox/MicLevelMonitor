using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace MicLevelMonitor
{
    public sealed class TrayApp : ApplicationContext
    {
        private readonly NotifyIcon notifyIcon;
        private readonly Timer updateTimer;
        private readonly Timer memoryCleanupTimer;
        private readonly MMDeviceEnumerator deviceEnumerator;
        private MMDevice micDevice;
        private bool isMonitoring;
        private const int IconSize = 32;
        private const int BarCount = 8;
        
        // Színséma - statikus, nem kell minden frissítésnél létrehozni
        private static readonly Color[] BarColors = {
            Color.FromArgb(0, 255, 0),     // Zöld (1-3)
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(255, 255, 0),   // Sárga (4-6)
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(255, 0, 0),     // Piros (7-8)
            Color.FromArgb(255, 0, 0)
        };

        public TrayApp()
        {
            // Tálcaikon inicializálása
            notifyIcon = new NotifyIcon
            {
                Text = "Mikrofon hangerő monitor",
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };
            notifyIcon.ContextMenuStrip.Items.Add("Kilépés", null, (s, e) => Exit());

            // Audio eszköz inicializálása
            deviceEnumerator = new MMDeviceEnumerator();
            InitializeMicrophone();

            // Időzítők beállítása
            updateTimer = new Timer { Interval = 50 };  // 20 FPS
            updateTimer.Tick += UpdateVolume;
            
            memoryCleanupTimer = new Timer { Interval = 30000 }; // 30 másodperc
            memoryCleanupTimer.Tick += (s, e) => GC.Collect(0, GCCollectionMode.Optimized);

            // Időzítők indítása
            updateTimer.Start();
            memoryCleanupTimer.Start();
        }

        private void InitializeMicrophone()
        {
            try
            {
                if (micDevice != null)
                {
                    micDevice.Dispose();
                    micDevice = null;
                }

                // Próbáljuk először a kommunikációs mikrofont
                micDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                isMonitoring = true;
                notifyIcon.Text = $"Mikrofon: {micDevice.FriendlyName}";
            }
            catch
            {
                try
                {
                    // Ha az nem megy, akkor a multimédia mikrofont
                    micDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
                    isMonitoring = true;
                    notifyIcon.Text = $"Mikrofon: {micDevice.FriendlyName}";
                }
                catch (Exception)
                {
                    isMonitoring = false;
                    notifyIcon.Text = "Nincs elérhető mikrofon";
                }
            }
        }

        private void UpdateVolume(object sender, EventArgs e)
        {
            if (!isMonitoring || micDevice?.AudioMeterInformation == null) return;

            try
            {
                // Hangerő lekérése (0.0 - 1.0)
                float level = micDevice.AudioMeterInformation.MasterPeakValue;
                
                // Százalékos érték számítása
                int volumePercent = (int)(level * 100);
                
                // Aktív sávok számítása (0-8)
                int activeBars = Math.Min(BarCount, (volumePercent * BarCount) / 100);
                
                DrawTrayIcon(activeBars);
                notifyIcon.Text = $"Mikrofon: {volumePercent}%";
            }
            catch
            {
                // Ha hiba van, próbáljuk újrainicializálni
                isMonitoring = false;
                InitializeMicrophone();
            }
        }

        private void DrawTrayIcon(int activeBars)
        {
            using var bmp = new Bitmap(IconSize, IconSize);
            using var g = Graphics.FromImage(bmp);
            
            // Háttér
            g.Clear(Color.Black);
            
            // Rajzolási paraméterek
            int margin = 3;
            int spacing = 1;
            int barWidth = IconSize - 2 * margin;
            int barHeight = (IconSize - 2 * margin - (BarCount - 1) * spacing) / BarCount;

            // Sávok rajzolása
            for (int i = 0; i < BarCount; i++)
            {
                int y = IconSize - margin - (i + 1) * barHeight - i * spacing;
                using var brush = new SolidBrush(i < activeBars ? BarColors[i] : Color.DimGray);
                g.FillRectangle(brush, margin, y, barWidth, barHeight);
            }

            // Keret
            using var framePen = new Pen(Color.White, 1);
            g.DrawRectangle(framePen, 0, 0, IconSize - 1, IconSize - 1);

            // Icon frissítése
            notifyIcon.Icon?.Dispose();
            IntPtr hIcon = bmp.GetHicon();
            notifyIcon.Icon = Icon.FromHandle(hIcon);
            DestroyIcon(hIcon);
        }

        private void Exit()
        {
            updateTimer?.Stop();
            memoryCleanupTimer?.Stop();
            Dispose();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                updateTimer?.Stop();
                updateTimer?.Dispose();
                memoryCleanupTimer?.Stop();
                memoryCleanupTimer?.Dispose();
                micDevice?.Dispose();
                deviceEnumerator?.Dispose();
                notifyIcon?.Icon?.Dispose();
                notifyIcon?.Dispose();
                GC.Collect(0, GCCollectionMode.Optimized);
            }
            base.Dispose(disposing);
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}