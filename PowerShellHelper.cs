using System.Diagnostics;

namespace ReWindows
{
    public static class PowerShellHelper
    {
        public static bool IsAppInstalled(string packageName)
        {
            string result = RunPowerShell($"Get-AppxPackage -Name '*{packageName}*' | Select-Object -ExpandProperty Name");
            return !string.IsNullOrWhiteSpace(result);
        }

        public static void RemoveApp(string packageName)
        {
            RunPowerShell($"Get-AppxPackage -Name '*{packageName}*' | Remove-AppxPackage");
        }

        public static void ReinstallApp(string winGetId)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = $"install --id {winGetId} --silent --accept-source-agreements --accept-package-agreements",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }

        private static string RunPowerShell(string command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            string output = process?.StandardOutput.ReadToEnd() ?? "";
            process?.WaitForExit();
            return output;
        }
    }
}