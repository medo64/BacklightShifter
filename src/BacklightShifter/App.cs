using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;

namespace BacklightShifter {
    internal static class App {

        [STAThread]
        internal static void Main(string[] args) {
            var parameter = string.Join(" ", args).TrimStart(new char[] { '/', '-' });

            if (parameter.Equals("interactive", StringComparison.InvariantCultureIgnoreCase)) {  // just for testing - no service necessary

                Tray.Show();
                ServiceWorker.Start();
                Tray.SetStatusToRunningInteractive();
                Application.Run();
                ServiceWorker.Stop();
                Tray.Hide();
                Environment.Exit(0);

            } else if (parameter.Equals("install", StringComparison.InvariantCultureIgnoreCase)) {  // install service

                try {
                    using (var sc = new ServiceController(AppService.Instance.ServiceName)) {
                        if (sc.Status != ServiceControllerStatus.Stopped) { sc.Stop(); }
                    }
                } catch (Exception) { }

                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                Environment.Exit(0);

            } else if (parameter.Equals("uninstall", StringComparison.InvariantCultureIgnoreCase)) {  // uninstall service

                try {
                    using (var sc = new ServiceController(AppService.Instance.ServiceName)) {
                        if (sc.Status != ServiceControllerStatus.Stopped) { sc.Stop(); }
                    }
                } catch (Exception) { }

                try {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                    Environment.Exit(0);
                } catch (InstallException) {  // no service with that name
                    Environment.Exit(1);
                }

            } else if (parameter.Equals("start", StringComparison.InvariantCultureIgnoreCase)) {  // start service

                try {
                    using (var service = new ServiceController("BacklightShifter")) {
                        if (service.Status != ServiceControllerStatus.Running) {
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 1));
                        }
                    }
                } catch (Exception) { }
                Environment.Exit(0);

            } else if (Environment.UserInteractive) {  // run tray status

                Tray.Show();
                ServiceStatusThread.Start();
                Application.Run();
                ServiceStatusThread.Stop();
                Tray.Hide();
                Environment.Exit(0);

            } else {  // you're running as a service

                ServiceBase.Run(new ServiceBase[] { AppService.Instance });

            }
        }

    }
}
