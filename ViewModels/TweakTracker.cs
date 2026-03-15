using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace ReWindows.ViewModels
{
    public static class TweakTracker
    {
        private static readonly string Path = "applied_tweaks.json";

        public static void SaveApplied(List<string> ids) =>
            File.WriteAllText(Path, JsonSerializer.Serialize(ids));

        public static List<string> LoadApplied() =>
            File.Exists(Path) ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Path)) ?? new() : new();
    }
}