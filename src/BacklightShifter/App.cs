using System;
using System.Windows.Forms;

namespace BacklightShifter {
    internal static class App {

        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }
}
