using System;
using System.Threading;
using System.Windows.Forms;

namespace MicLevelMonitor
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Mutex létrehozása a dupla futás megakadályozására
            using (var mutex = new Mutex(true, "MicLevelMonitor_SingleInstance", out bool createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new TrayApp());
                }
                else
                {
                    // Ha már fut, ne indítson újat
                    MessageBox.Show("A Mikrofon Monitor már fut!", "Figyelmeztetés",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}