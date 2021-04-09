using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

namespace BacklightShifter.Service {
    internal static class ServiceWorker {

        public static void Start() {
            Thread.Start();
        }

        public static void Stop() {
            CancelEvent.Set();
        }


        private static readonly ManualResetEvent CancelEvent = new ManualResetEvent(false);
        private static readonly Thread Thread = new Thread(Run) { Name = "Service Worker" };

        private static void Run() {
            var stopwatchDelaySave = new Stopwatch();

            var lastStatus = PowerLineStatus.Unknown;
            while (!CancelEvent.WaitOne(500)) {
                var currStatus = PowerStatus.Current;
                var currLevel = Backlight.Level;
                var storedLevel = Storage.GetLevel(currStatus, currLevel);

                if (currStatus != lastStatus) {
                    Debug.WriteLine($"[Worker] Status change: {lastStatus} -> {currStatus}");
                    if (storedLevel != currLevel) {
                        Backlight.Level = storedLevel;
                        Debug.WriteLine($"[Worker] Level restored: {storedLevel}% ({currStatus} was {currLevel}%)");
                        stopwatchDelaySave.Restart();
                    }
                } else if (stopwatchDelaySave.IsRunning && (stopwatchDelaySave.ElapsedMilliseconds > 1000)) {  // do it again as sometime it doesn't "take"
                    if (currLevel != storedLevel) {
                        Backlight.Level = storedLevel;
                        Debug.WriteLine($"[Worker] Level restored (2): {storedLevel}% ({currStatus} was {currLevel}%)");
                    }
                    stopwatchDelaySave.Stop();  // re-enable saving
                } else if (!stopwatchDelaySave.IsRunning && (currLevel != storedLevel)) {  // save
                    Storage.SetLevel(currStatus, currLevel);
                    Debug.WriteLine($"[Worker] Level stored: {currLevel}% ({currStatus})");
                }
                lastStatus = currStatus;
            }
        }


        #region PowerStatus

        private static class PowerStatus {

            public static PowerLineStatus Current {
                get { return SystemInformation.PowerStatus.PowerLineStatus; }
            }

        }

        #endregion PowerStatus

        #region Backlight

        private static class Backlight {  // https://docs.microsoft.com/en-us/windows/win32/power/backlight-control-interface

            public static int Level {
                get {
                    var lcdHandle = GetLcdHandle();
                    try {
                        if (GetBrightness(lcdHandle, out var brightness)) {
                            return brightness.ucACBrightness;  // just use AC
                        }
                    } finally {
                        lcdHandle.Close();
                    }
                    return -1;
                }
                set {
                    if ((value < 0) || (value > 255)) { return; }  // ignore out of range
                    var lcdHandle = GetLcdHandle();
                    try {
                        if (GetBrightness(lcdHandle, out var brightness)) {
                            brightness.ucACBrightness = (byte)value;
                            brightness.ucDCBrightness = (byte)value;
                            SetBrightness(lcdHandle, brightness);
                        }
                    } finally {
                        lcdHandle.Close();
                    }
                }
            }


            private static SafeFileHandle GetLcdHandle() {
                return NativeMethods.CreateFile(
                    @"\\.\LCD",
                    NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
                    0,
                    IntPtr.Zero,
                    NativeMethods.OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                );
            }

            private static bool GetBrightness(SafeFileHandle lcdHandle, out NativeMethods.DISPLAY_BRIGHTNESS brightness) {
                brightness = new NativeMethods.DISPLAY_BRIGHTNESS();
                if (lcdHandle.IsInvalid) { return false; }

                if (NativeMethods.DeviceIoControl(
                    lcdHandle,
                    NativeMethods.IOCTL_VIDEO_QUERY_DISPLAY_BRIGHTNESS,
                    IntPtr.Zero,
                    0,
                    out brightness,
                    Marshal.SizeOf(brightness),
                    out _,
                    IntPtr.Zero
                )) {
                    return true;
                }
                return false;
            }

            private static bool SetBrightness(SafeFileHandle lcdHandle, NativeMethods.DISPLAY_BRIGHTNESS brightness) {
                if (lcdHandle.IsInvalid) { return false; }

                if (NativeMethods.DeviceIoControl(
                    lcdHandle,
                    NativeMethods.IOCTL_VIDEO_SET_DISPLAY_BRIGHTNESS,
                    ref brightness,
                    Marshal.SizeOf(brightness),
                    IntPtr.Zero,
                    0,
                    out _,
                    IntPtr.Zero
                )) {
                    return true;
                }
                return false;
            }


            private static class NativeMethods {

                public const uint GENERIC_READ = 0x80000000;
                public const uint GENERIC_WRITE = 0x40000000;
                public const uint OPEN_EXISTING = 3;
                public const int IOCTL_VIDEO_QUERY_DISPLAY_BRIGHTNESS = 0x230498;
                public const int IOCTL_VIDEO_SET_DISPLAY_BRIGHTNESS = 0x23049c;

                [DebuggerDisplay("{ucACBrightness}")]
                public struct DISPLAY_BRIGHTNESS {
                    public byte ucDisplayPolicy;
                    public byte ucACBrightness;
                    public byte ucDCBrightness;
                }

                [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
                public static extern SafeFileHandle CreateFile(
                    string lpFileName,
                    uint dwDesiredAccess,
                    uint dwShareMode,
                    IntPtr lpSecurityAttributes,
                    uint dwCreationDisposition,
                    uint dwFlagsAndAttributes,
                    IntPtr hTemplateFile
                );

                [DllImport("Kernel32.dll", SetLastError = true)]
                public static extern bool DeviceIoControl(
                    SafeFileHandle hDevice,
                    int IoControlCode,
                    IntPtr InBuffer,
                    int nInBufferSize,
                    out DISPLAY_BRIGHTNESS OutBuffer,
                    int nOutBufferSize,
                    out int pBytesReturned,
                    IntPtr Overlapped
                );

                [DllImport("Kernel32.dll", SetLastError = true)]
                public static extern bool DeviceIoControl(
                    SafeFileHandle hDevice,
                    int IoControlCode,
                    ref DISPLAY_BRIGHTNESS InBuffer,
                    int nInBufferSize,
                    IntPtr OutBuffer,
                    int nOutBufferSize,
                    out int pBytesReturned,
                    IntPtr Overlapped
                );
            }

        }

        #endregion Backlight

    }
}
