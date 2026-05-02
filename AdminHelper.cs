using System;
using System.Diagnostics;
using System.Security.Principal;

namespace ReWindows
{
    public static class AdminHelper
    {
        public static bool IsRunningAsAdmin()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool RestartAsAdmin()
        {
            string? exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath))
            {
                var mainModule = Process.GetCurrentProcess().MainModule;
                exePath = mainModule?.FileName;
            }

            if (string.IsNullOrEmpty(exePath))
                return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                var process = Process.Start(startInfo);
                if (process is null)
                    return false;

                Environment.Exit(0);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}