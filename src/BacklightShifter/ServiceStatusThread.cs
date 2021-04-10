using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceProcess;
using System.Threading;

namespace BacklightShifter {
    internal static class ServiceStatusThread {

        private static Thread Thread;
        private static readonly ManualResetEvent CancelEvent = new ManualResetEvent(false);


        public static void Start() {
            if (Thread != null) { return; }

            Thread = new Thread(Run) {
                Name = "Service status",
                CurrentCulture = CultureInfo.InvariantCulture
            };
            Thread.Start();
        }

        public static void Stop() {
            try {
                CancelEvent.Set();
                while (Thread.IsAlive) { Thread.Sleep(10); }
                Thread = null;
                CancelEvent.Reset();
            } catch { }
        }


        private static void Run() {
            try {
                using (var service = new ServiceController(AppService.Instance.ServiceName)) {
                    bool? lastIsRunning = null;
                    Tray.SetStatusToUnknown();

                    while (!CancelEvent.WaitOne(250, false)) {
                        bool? currIsRunning;
                        try {
                            service.Refresh();
                            currIsRunning = (service.Status == ServiceControllerStatus.Running);
                        } catch (InvalidOperationException) {
                            currIsRunning = null;
                        }
                        if (lastIsRunning != currIsRunning) {
                            if (currIsRunning == null) {
                                Tray.SetStatusToUnknown();
                            } else if (currIsRunning == true) {
                                Tray.SetStatusToRunning();
                            } else {
                                Tray.SetStatusToStopped();
                            }
                        }
                        lastIsRunning = currIsRunning;
                    }
                }
            } catch (ThreadAbortException) { }
        }

    }
}
