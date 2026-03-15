using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ReWindows.ViewModels
{
    public enum TweakSafety { Safe, Moderate, Dangerous }

    public partial class Tweak : ObservableObject
    {
        public string Id { get; set; } = ""; // Unique ID for JSON tracking
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public TweakSafety Safety { get; set; } = TweakSafety.Safe;

        [ObservableProperty] private bool _isChecked;
        [ObservableProperty] private bool _isAlreadyApplied; // Found in registry
        [ObservableProperty] private bool _isAppliedByApp;   // Found in our JSON tracking

        public Action? RunAction { get; set; }
        public Action? RevertAction { get; set; }
        public Func<bool>? CheckAction { get; set; }
    }
}