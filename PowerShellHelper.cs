using System;
using System.Diagnostics;
using System.Linq;

namespace ReWindows
{
    public sealed record CommandResult(bool Success, int ExitCode, string StandardOutput, string StandardError)
    {
        public string CombinedOutput => string.Join(
            Environment.NewLine,
            new[] { StandardOutput, StandardError }.Where(text => !string.IsNullOrWhiteSpace(text)));
    }

    public static class PowerShellHelper
    {
        public static bool IsAppInstalled(string packageName)
        {
            var result = RunPowerShell($"Get-AppxPackage -Name '{EscapePowerShellLiteral(packageName)}' | Select-Object -ExpandProperty Name");
            return result.Success && !string.IsNullOrWhiteSpace(result.StandardOutput);
        }

        public static CommandResult RemoveApp(string packageName)
        {
            return RunPowerShell($"Get-AppxPackage -Name '{EscapePowerShellLiteral(packageName)}' | Remove-AppxPackage");
        }

        public static CommandResult ReinstallApp(string winGetId, string wingetSource)
        {
            if (string.IsNullOrWhiteSpace(winGetId))
                return new CommandResult(false, -1, string.Empty, "Missing winget package ID.");

            if (string.IsNullOrWhiteSpace(wingetSource))
                return new CommandResult(false, -1, string.Empty, $"Missing winget source for {winGetId}.");

            return RunProcess("winget", $"install --id \"{EscapeProcessArgument(winGetId)}\" --source {wingetSource} --exact --silent --accept-source-agreements --accept-package-agreements");
        }

        private static CommandResult RunPowerShell(string command)
        {
            return RunProcess("powershell.exe", $"-NoProfile -NonInteractive -Command \"{command}\"");
        }

        private static CommandResult RunProcess(string fileName, string arguments, int timeoutMs = 30000)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process is null)
                    return new CommandResult(false, -1, string.Empty, $"Failed to start {fileName}.");

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { }
                    return new CommandResult(false, -1, string.Empty, $"Process timed out after {timeoutMs / 1000} seconds.");
                }

                return new CommandResult(process.ExitCode == 0, process.ExitCode, output, error);
            }
            catch (Exception ex)
            {
                return new CommandResult(false, -1, string.Empty, ex.Message);
            }
        }

        private static string EscapePowerShellLiteral(string value)
        {
            return value.Replace("'", "''");
        }

        private static string EscapeProcessArgument(string value)
        {
            return value.Replace("\"", "\\\"");
        }
    }
}
