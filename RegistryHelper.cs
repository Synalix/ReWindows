using Microsoft.Win32;
using System.Runtime.Versioning;

public static class RegistryHelper
{
    public static void SetDword(string path, string keyName, int value)
    {
        string root = path.Split('\\')[0];
        string subKey = path.Substring(root.Length + 1);
        RegistryKey baseKey = root == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;

        using (var key = baseKey.CreateSubKey(subKey, true))
        {
            key?.SetValue(keyName, value, RegistryValueKind.DWord);
        }
    }

    public static int GetDword(string path, string keyName, int defaultValue = 0)
    {
        try
        {
            string root = path.Split('\\')[0];
            string subKey = path.Substring(root.Length + 1);
            RegistryKey baseKey = root == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;

            using (var key = baseKey.OpenSubKey(subKey))
            {
                var val = key?.GetValue(keyName);
                return val is int i ? i : defaultValue;
            }
        }
        catch { return defaultValue; }
    }
}