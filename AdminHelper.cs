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

        public static void RestartAsAdmin()
        {
            string exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule!.FileName;

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}