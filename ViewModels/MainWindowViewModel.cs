using Avalonia.Controls.Notifications;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ReWindows.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        
        public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();

        public ObservableCollection<Tweak> AllTweaks { get; set; } = new();

        [ObservableProperty] private string _searchText = "";

        public IEnumerable<Tweak> SystemTweaks => FilterTweaks("System");
        public IEnumerable<Tweak> PrivacyTweaks => FilterTweaks("Privacy");
        public IEnumerable<Tweak> PerformanceTweaks => FilterTweaks("Performance");

        public MainWindowViewModel()
        {
            InitializeTweaks();
            RefreshTweakStates();
        }

        private IEnumerable<Tweak> FilterTweaks(string category) =>
            AllTweaks.Where(t => t.Category == category &&
                (string.IsNullOrEmpty(SearchText) || t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

        partial void OnSearchTextChanged(string value)
        {
            OnPropertyChanged(nameof(SystemTweaks));
            OnPropertyChanged(nameof(PrivacyTweaks));
            OnPropertyChanged(nameof(PerformanceTweaks));
        }

        private void InitializeTweaks()
        {
            
            var tweaks = new List<Tweak>
            {
                new Tweak { Id = "telemetry", Name = "Disable Telemetry", Description = "Prevents Windows from collecting usage data.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0),
                    CheckAction = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1) == 0 },
                new Tweak { Id = "advertising", Name = "Disable Advertising ID", Description = "Prevents apps from using your ID for targeted ads.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0),
                    CheckAction = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1) == 0 },
                new Tweak { Id = "transparency", Name = "Disable Transparency", Description = "Disables acrylic/glass effects to save GPU resources.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0),
                    CheckAction = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1) == 0 },
                new Tweak { Id = "gamebar", Name = "Disable Game Bar", Description = "Stops the Xbox Game Bar from running in the background.", Category = "Performance", Safety = TweakSafety.Moderate,
                    RunAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
                    CheckAction = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 1) == 0 },
                new Tweak { Id = "extensions", Name = "Show File Extensions", Description = "Forces File Explorer to show extensions like .exe or .txt.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0),
                    CheckAction = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1) == 0 },
                new Tweak { Id = "bing", Name = "Disable Bing in Start", Description = "Removes web search results from the Start menu.", Category = "System", Safety = TweakSafety.Dangerous,
                    RunAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1),
                    CheckAction = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 0) == 1 }
            };
            foreach (var t in tweaks) AllTweaks.Add(t);
        }

        private void RefreshTweakStates()
        {
            var appliedByApp = TweakTracker.LoadApplied();
            foreach (var tweak in AllTweaks)
            {
                bool isActive = tweak.CheckAction?.Invoke() ?? false;
                tweak.IsAppliedByApp = appliedByApp.Contains(tweak.Id);
                tweak.IsAlreadyApplied = isActive && !tweak.IsAppliedByApp;
                tweak.IsChecked = isActive;
            }
            var sorted = AllTweaks.OrderBy(t => t.IsAlreadyApplied).ThenBy(t => t.Name).ToList();
            AllTweaks.Clear();
            foreach (var item in sorted) AllTweaks.Add(item);
        }

        [RelayCommand]
        private void GlobalSelect(string param)
        {
            foreach (var t in AllTweaks.Where(x => !x.IsAlreadyApplied))
            {
                if (param == "Safe") t.IsChecked = t.Safety == TweakSafety.Safe;
                else if (param == "None") t.IsChecked = false;
                else if (param == "All") t.IsChecked = true;
            }
        }

        [RelayCommand]
        public void ApplyWithConfirmation()
        {
            
            bool hasDangerous = AllTweaks.Any(t => t.IsChecked && !t.IsAlreadyApplied && t.Safety == TweakSafety.Dangerous);

            var builder = DialogManager.CreateDialog()
                .WithTitle("Confirm Actions")
                .WithContent(hasDangerous
                    ? "Warning: You have selected dangerous tweaks that may affect system stability. Proceed?"
                    : "Are you sure you want to apply the selected optimizations?")
                .WithActionButton("Cancel", _ => { }, true)
                .WithActionButton("Apply", _ => ExecuteApply(), true, "Flat", "Accent");

            
            if (hasDangerous)
            {
                builder.OfType(NotificationType.Warning);
            }

            builder.TryShow();
        }

        private async void ExecuteApply()
        {
            var tracker = TweakTracker.LoadApplied();
            await Task.Run(() =>
            {
                foreach (var t in AllTweaks.Where(t => t.IsChecked && !t.IsAlreadyApplied))
                {
                    try { t.RunAction?.Invoke(); if (!tracker.Contains(t.Id)) tracker.Add(t.Id); } catch { }
                }
                TweakTracker.SaveApplied(tracker);
            });
            RefreshTweakStates();
        }
    }
}