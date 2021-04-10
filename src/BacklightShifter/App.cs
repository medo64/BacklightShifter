using System;
using System.Windows.Forms;

namespace BacklightShifter {
    internal static class App {

        [STAThread]
        internal static void Main() {
            Tray.Show();
            ServiceWorker.Start();
            Tray.SetStatusToRunningInteractive();
            Application.Run();
            ServiceWorker.Stop();
            Tray.Hide();
            Environment.Exit(0);
        }

    }
}
