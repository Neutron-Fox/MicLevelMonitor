using System;
using System.Windows.Forms;

namespace MicLevelMonitor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Konzol ablak megjelenítése debug-hoz
            AllocConsole();
            Console.WriteLine("=== MIKROFON MONITOR (WaveIn) ===");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApp());

            FreeConsole();
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
    }
}