using System;
using System.Threading;
using System.Windows.Forms;

namespace MicLevelMonitor
{
    internal static class Program
    {
        private const string MutexName = "MicLevelMonitor_SingleInstance";

        [STAThread]
        static void Main()
        {
            // Alkalmazás beállítások - startup optimalizálás
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Mutex single instance check
            using (var mutex = new Mutex(true, MutexName, out bool createdNew))
            {
                if (createdNew)
                {
                    try
                    {
                        // Memória optimalizálás startup-kor
                        GC.Collect(0, GCCollectionMode.Optimized);

                        Application.Run(new TrayApp());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Alkalmazás hiba: {ex.Message}", "Hiba",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Ha már fut, csendes kilépés (nincs MessageBox)
            }
        }
    }
}