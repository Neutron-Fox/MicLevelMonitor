using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MicLevelMonitor
{
    internal static class Program
    {
        private const string MutexName = "MicLevelMonitor_SingleInstance";

        [STAThread]
        static void Main()
        {
            // Gyors startup beállítások
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Process prioritás csökkentés a gyorsabb indításért
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass =
                System.Diagnostics.ProcessPriorityClass.BelowNormal;

            // Mutex single instance check
            using (var mutex = new Mutex(true, MutexName, out bool createdNew))
            {
                if (createdNew)
                {
                    try
                    {
                        // Minimal GC - csak startup optimalizálás
                        GC.Collect(0, GCCollectionMode.Optimized);

                        // Session change event kezelés
                        SystemEvents.SessionSwitch += OnSessionSwitch;

                        Application.Run(new TrayApp());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Alkalmazás hiba: {ex.Message}", "Hiba",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        SystemEvents.SessionSwitch -= OnSessionSwitch;
                    }
                }
                // Ha már fut, csendes kilépés
            }
        }

        private static void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            // Session váltás/unlock esetén refresh
            if (e.Reason == SessionSwitchReason.SessionUnlock ||
                e.Reason == SessionSwitchReason.SessionLogon)
            {
                // Trigger refresh az alkalmazásban
                GC.Collect(0, GCCollectionMode.Optimized);
            }
        }
    }
}