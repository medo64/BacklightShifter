using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;

namespace BacklightShifter {
    [RunInstaller(true)]
    public class AppServiceInstaller : Installer {

        public AppServiceInstaller() {
            var processInstaller = new ServiceProcessInstaller {
                Account = ServiceAccount.LocalService,
                Username = null,
                Password = null
            };
            Installers.Add(processInstaller);

            var installer = new ServiceInstaller {
                ServiceName = AppService.Instance.ServiceName,
                DisplayName = Medo.Reflection.EntryAssembly.Product,
                Description = Medo.Reflection.EntryAssembly.Description,
                StartType = ServiceStartMode.Automatic
            };
            Installers.Add(installer);
        }


        protected override void OnCommitted(IDictionary savedState) {
            Debug.WriteLine("OnCommitted : Begin.");
            base.OnCommitted(savedState);
            using (var sc = new ServiceController(AppService.Instance.ServiceName)) {
                Debug.WriteLine("OnCommitted : Service starting...");
                sc.Start();
                Debug.WriteLine("OnCommitted : Service started.");
            }
            Debug.WriteLine("OnCommitted : End.");
        }

        protected override void OnBeforeUninstall(IDictionary savedState) {
            Debug.WriteLine("OnBeforeUninstall : Begin.");
            using (var sc = new ServiceController(AppService.Instance.ServiceName)) {
                if (sc.Status != ServiceControllerStatus.Stopped) {
                    Debug.WriteLine("OnBeforeUninstall : Service stopping...");
                    sc.Stop();
                    Debug.WriteLine("OnBeforeUninstall : Service stopped...");
                }
            }
            base.OnBeforeUninstall(savedState);
            Debug.WriteLine("OnBeforeUninstall : End.");
        }

    }
}
