using System;
using System.Windows.Forms;

namespace MicLevelMonitor
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using var app = new TrayApp();
                Application.Run(app);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Program hiba: {ex.Message}", "Hiba", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}