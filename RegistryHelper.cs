using Microsoft.Win32;

namespace ReWindows
{
    public static class RegistryHelper
    {
        public static void SetDword(string path, string keyName, int value)
        {
            string root = path.Split('\\')[0];
            string subKey = path[(root.Length + 1)..];
            RegistryKey baseKey = root == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;

            using var key = baseKey.CreateSubKey(subKey, true);
            key?.SetValue(keyName, value, RegistryValueKind.DWord);
        }

        public static int GetDword(string path, string keyName, int defaultValue = 0)
        {
            try
            {
                string root = path.Split('\\')[0];
                string subKey = path[(root.Length + 1)..];
                RegistryKey baseKey = root == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;

                using var key = baseKey.OpenSubKey(subKey);
                var val = key?.GetValue(keyName);
                return val is int i ? i : defaultValue;
            }
            catch { return defaultValue; }
        }

        public static void SetString(string path, string keyName, string value)
        {
            string root = path.Split('\\')[0];
            string subKey = path[(root.Length + 1)..];
            RegistryKey baseKey = root == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;

            using var key = baseKey.CreateSubKey(subKey, true);
            key?.SetValue(keyName, value, RegistryValueKind.String);
        }

        public static string GetString(string path, string keyName, string defaultValue = "")
        {
            try
            {
                string root = path.Split('\\')[0];
                string subKey = path[(root.Length + 1)..];
                RegistryKey baseKey = root == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;

                using var key = baseKey.OpenSubKey(subKey);
                var val = key?.GetValue(keyName);
                return val is string s ? s : defaultValue;
            }
            catch { return defaultValue; }
        }
    }
}