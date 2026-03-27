using System;
using System.IO;
using System.Text.Json;

namespace ReWindows.ViewModels
{
    public class AppSettings
    {
        public bool IsDarkMode { get; set; } = true;
        public string BackgroundStyle { get; set; } = "Gradient";
    }

    public static class TweakTracker
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReWindows");

        private static readonly string SettingsFile = Path.Combine(FolderPath, "settings.json");

        public static void SaveSettings(AppSettings settings)
        {
            Directory.CreateDirectory(FolderPath);
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(settings));
        }

        public static AppSettings LoadSettings()
        {
            if (!File.Exists(SettingsFile))
                return new AppSettings();

            string json = File.ReadAllText(SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }
}