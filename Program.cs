using System;
using System.Threading;
using System.Windows.Forms;

namespace MicLevelMonitor
{
    internal static class Program
    {
        private static Mutex? mutex = null;
        private const string AppName = "MicLevelMonitor";

        /// <summary>
        /// Az alkalmazás fő belépési pontja.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;

            mutex = new Mutex(true, AppName, out createdNew);

            if (!createdNew)
            {
                // Program már fut - kilépés nélküli visszatérés
                return;
            }

            try
            {
                // High DPI támogatás
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Alkalmazás indítása
                using var app = new TrayApp();
                Application.Run(app);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kritikus hiba történt: {ex.Message}", 
                    "Mikrofon Monitor - Hiba", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
            finally
            {
                mutex?.ReleaseMutex();
                mutex?.Dispose();
            }
        }
    }
}