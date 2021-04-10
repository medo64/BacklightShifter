using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BacklightShifter {
    internal static class Tray {

        private static NotifyIcon Notification;

        internal static void Show() {
            Notification = new NotifyIcon {
                Icon = GetApplicationIcon(),
                Text = Medo.Reflection.EntryAssembly.Title,
                Visible = true,
                ContextMenu = new ContextMenu()
            };
            Notification.ContextMenu.MenuItems.Add(
                new MenuItem("Exit", delegate { Application.Exit(); } )
            );
        }

        internal static void SetStatusToRunningInteractive() {
            Notification.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(Medo.Reflection.EntryAssembly.Name + ".Resources.Service_RunningInteractive_12.png")));
            Notification.Text = Medo.Reflection.EntryAssembly.Title + " (PID=" + Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture) + ")";
        }

        internal static void SetStatusToUnknown() {
            Notification.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(Medo.Reflection.EntryAssembly.Name + ".Resources.Service_Unknown_12.png")));
            Notification.Text = Medo.Reflection.EntryAssembly.Title + " - Unknown state.";
        }

        internal static void SetStatusToRunning() {
            Notification.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(Medo.Reflection.EntryAssembly.Name + ".Resources.Service_Running_12.png")));
            Notification.Text = Medo.Reflection.EntryAssembly.Title + " - Running.";
        }

        internal static void SetStatusToStopped() {
            Notification.Icon = GetAnnotatedIcon(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(Medo.Reflection.EntryAssembly.Name + ".Resources.Service_Stopped_12.png")));
            Notification.Text = Medo.Reflection.EntryAssembly.Title + " - Stopped.";
        }

        internal static void Hide() {
            Notification.Visible = false;
        }


        #region Helpers

        private static Icon GetAnnotatedIcon(Image annotation) {
            var icon = GetApplicationIcon();

            if (icon != null) {
                var image = icon.ToBitmap();
                if (icon != null) {
                    using (var g = Graphics.FromImage(image)) {
                        g.DrawImage(annotation, (int)g.VisibleClipBounds.Width - annotation.Width - 2, (int)g.VisibleClipBounds.Height - annotation.Height - 2);
                        g.Flush();
                    }
                }
                return Icon.FromHandle(image.GetHicon());
            }
            return null;
        }

        private static Icon GetApplicationIcon() {
            IntPtr hLibrary = NativeMethods.LoadLibrary(Assembly.GetEntryAssembly().Location);
            if (!hLibrary.Equals(IntPtr.Zero)) {
                IntPtr hIcon = NativeMethods.LoadImage(hLibrary, "#32512", NativeMethods.IMAGE_ICON, 20, 20, 0);
                if (!hIcon.Equals(System.IntPtr.Zero)) {
                    Icon icon = Icon.FromHandle(hIcon);
                    if (icon != null) { return icon; }
                }
            }
            return null;
        }


        private static class NativeMethods {

            public const UInt32 IMAGE_ICON = 1;


            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadImage(IntPtr hInstance, String lpIconName, UInt32 uType, Int32 cxDesired, Int32 cyDesired, UInt32 fuLoad);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(String lpFileName);

        }

        #endregion

    }
}
