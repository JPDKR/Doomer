using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Doomer
{
    internal static class ForegroundWindow
    {
        // Whether the window in front belongs to another app that started at or after since: a game just launched,
        // rather than a window the user switched to or one of Doomer's own dialogs.
        public static bool IsFromProcessStartedSince(DateTime since)
        {
            var window = GetForegroundWindow();

            if (window == IntPtr.Zero)
                return false;

            GetWindowThreadProcessId(window, out var processId);

            if (processId == 0 || processId == Environment.ProcessId)
                return false;

            try
            {
                using var process = Process.GetProcessById((int)processId);
                return process.StartTime >= since;
            }
            // Gone already, or one Doomer isn't allowed to look at (e.g. running as administrator).
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or Win32Exception)
            {
                return false;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    }
}
