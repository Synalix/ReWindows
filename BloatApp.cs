using CommunityToolkit.Mvvm.ComponentModel;

namespace ReWindows.ViewModels
{
    public enum AppCategory { Microsoft, AI, Games, ThirdParty }

    public partial class BloatApp : ObservableObject
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public AppCategory Category { get; set; }
        public string PackageName { get; set; } = "";
        public string WinGetId { get; set; } = "";
        public string WingetSource { get; set; } = "";

        [ObservableProperty] private bool _isChecked;
        [ObservableProperty] private bool _isInstalled;
    }
}