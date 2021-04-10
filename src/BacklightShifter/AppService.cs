using System.Diagnostics;
using System.ServiceProcess;

namespace BacklightShifter {
    internal class AppService : ServiceBase {

        public static AppService Instance { get; } = new AppService();


        private AppService() {
            AutoLog = true;
            CanStop = true;
            ServiceName = "BacklightShifter";
        }

        protected override void OnStart(string[] args) {
            Debug.WriteLine("AppService : Start requested.");
            ServiceWorker.Start();
        }

        protected override void OnStop() {
            Debug.WriteLine("AppService : Stop requested.");
            ServiceWorker.Stop();
        }

    }
}
