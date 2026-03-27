using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
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
        public SettingsViewModel Settings { get; } = new();
        public bool IsAdmin { get; } = AdminHelper.IsRunningAsAdmin();

        public ObservableCollection<Tweak> AllTweaks { get; } = new();
        public ObservableCollection<BloatApp> AllApps { get; } = new();

        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private object? _selectedPage;

        private bool _debloatLoaded = false;

        public IEnumerable<Tweak> SystemTweaks => FilterTweaks("System");
        public IEnumerable<Tweak> PrivacyTweaks => FilterTweaks("Privacy");
        public IEnumerable<Tweak> PerformanceTweaks => FilterTweaks("Performance");

        public IEnumerable<BloatApp> MicrosoftApps => FilterApps(AppCategory.Microsoft);
        public IEnumerable<BloatApp> AIApps => FilterApps(AppCategory.AI);
        public IEnumerable<BloatApp> GameApps => FilterApps(AppCategory.Games);
        public IEnumerable<BloatApp> ThirdPartyApps => FilterApps(AppCategory.ThirdParty);

        public bool IsOnTweaksPage => GetCurrentTabHeader() is null or "System Tweaks" or "Privacy" or "Performance";
        public bool IsOnDebloatPage => GetCurrentTabHeader() == "Debloat";

        public MainWindowViewModel()
        {
            InitializeTweaks();
            RefreshTweakStates();
            InitializeApps();
        }

        private string? GetCurrentTabHeader()
        {
            if (SelectedPage is SukiSideMenuItem tab)
                return tab.Header?.ToString();
            return null;
        }

        partial void OnSelectedPageChanged(object? value)
        {
            OnPropertyChanged(nameof(IsOnTweaksPage));
            OnPropertyChanged(nameof(IsOnDebloatPage));

            if (IsOnDebloatPage && !_debloatLoaded)
            {
                RefreshAppStates();
                _debloatLoaded = true;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            OnPropertyChanged(nameof(SystemTweaks));
            OnPropertyChanged(nameof(PrivacyTweaks));
            OnPropertyChanged(nameof(PerformanceTweaks));
            OnPropertyChanged(nameof(MicrosoftApps));
            OnPropertyChanged(nameof(AIApps));
            OnPropertyChanged(nameof(GameApps));
            OnPropertyChanged(nameof(ThirdPartyApps));
            OnPropertyChanged(nameof(IsOnDebloatPage));
        }

        private IEnumerable<Tweak> FilterTweaks(string category)
        {
            return AllTweaks.Where(t =>
                t.Category == category &&
                (string.IsNullOrEmpty(SearchText) || t.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)));
        }

        private IEnumerable<BloatApp> FilterApps(AppCategory category)
        {
            return AllApps.Where(a =>
        a.Category == category &&
        (string.IsNullOrEmpty(SearchText) ||
         a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        private void InitializeTweaks()
        {
            var tweaks = new List<Tweak>
            {
                new Tweak { Id = "telemetry", Name = "Disable Telemetry", Description = "Prevents Windows from collecting usage data.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1) == 0 },

                new Tweak { Id = "advertising", Name = "Disable Advertising ID", Description = "Prevents apps from using your ID for targeted ads.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1) == 0 },

                new Tweak { Id = "transparency", Name = "Disable Transparency", Description = "Disables acrylic/glass effects to save GPU resources.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1) == 0 },

                new Tweak { Id = "gamebar", Name = "Disable Game Bar", Description = "Stops the Xbox Game Bar from running in the background.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 1) == 0 },

                new Tweak { Id = "extensions", Name = "Show File Extensions", Description = "Forces File Explorer to show extensions like .exe or .txt.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1) == 0 },

                new Tweak { Id = "bing", Name = "Disable Bing in Start", Description = "Removes web search results from the Start menu.", Category = "System", Safety = TweakSafety.Moderate,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 0) == 1 },

                new Tweak { Id = "cortana", Name = "Disable Cortana", Description = "Prevents Cortana from running in the background.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 1) == 0 },

                new Tweak { Id = "activityhistory", Name = "Disable Activity History", Description = "Stops Windows from tracking apps and files you open.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 1) == 0 },

                new Tweak { Id = "locationtracking", Name = "Disable Location Tracking", Description = "Prevents Windows from tracking your device location.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetString(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny"),
                    RevertAction = () => RegistryHelper.SetString(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Allow"),
                    CheckAction  = () => RegistryHelper.GetString(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Allow") == "Deny" },

                new Tweak { Id = "feedbackfrequency", Name = "Disable Feedback Requests", Description = "Stops Windows from asking for diagnostic feedback.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 1) == 0 },

                new Tweak { Id = "animationsoff", Name = "Disable Animations", Description = "Turns off window animations for a snappier feel.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", 1) == 0 },

                new Tweak { Id = "hibernation", Name = "Disable Hibernation", Description = "Frees up disk space by removing the hibernation file.", Category = "Performance", Safety = TweakSafety.Moderate,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1) == 0 },

                new Tweak { Id = "fastboot", Name = "Enable Fast Startup", Description = "Speeds up boot time by saving system state on shutdown.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0) == 1 },

                new Tweak { Id = "hiddensysfiles", Name = "Show Hidden Files", Description = "Makes hidden files and folders visible in File Explorer.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 2),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 2) == 1 },

                new Tweak { Id = "tailoredexperiences", Name = "Disable Tailored Experiences", Description = "Stops Windows from using diagnostic data to show personalized tips and ads.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 1) == 0 },

                new Tweak { Id = "speechrecognition", Name = "Disable Online Speech Recognition", Description = "Prevents Windows from sending your voice data to Microsoft servers.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 1) == 0 },

                new Tweak { Id = "inkingtyping", Name = "Disable Inking & Typing Data", Description = "Stops Windows from collecting your typing and handwriting patterns.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 0) == 1 },

                new Tweak { Id = "consumerfeatures", Name = "Disable Consumer Features", Description = "Prevents Windows from auto-installing sponsored or suggested apps.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 0) == 1 },

                new Tweak { Id = "searchhighlights", Name = "Disable Search Highlights", Description = "Removes trending and curated web content from the search box.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 1) == 0 },

                new Tweak { Id = "searchboxsuggestions", Name = "Disable Start Menu Suggestions", Description = "Removes app suggestions and recommendations from the Start menu.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 1) == 0 },

                new Tweak { Id = "widgets", Name = "Disable Widgets", Description = "Removes the Widgets button from the taskbar and stops its background process.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 1) == 0 },

                new Tweak { Id = "stickykeys", Name = "Disable Sticky Keys Prompt", Description = "Stops the Sticky Keys dialog from appearing when Shift is pressed 5 times.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", 506),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", 510),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", 510) == 506 },

                new Tweak { Id = "windowstips", Name = "Disable Windows Tips", Description = "Stops Windows from showing tips, tricks and suggestions on the lock screen and desktop.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1) == 0 },

                new Tweak { Id = "lockscreenads", Name = "Disable Lock Screen Ads", Description = "Prevents Windows Spotlight from showing ads and suggestions on the lock screen.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1) == 0 },
            };

            foreach (var tweak in tweaks)
                AllTweaks.Add(tweak);
        }

        private void RefreshTweakStates()
        {
            foreach (var tweak in AllTweaks)
                tweak.IsApplied = tweak.CheckAction?.Invoke() ?? false;

            var sorted = AllTweaks.OrderBy(t => t.Name).ToList();
            AllTweaks.Clear();
            foreach (var tweak in sorted)
                AllTweaks.Add(tweak);
        }

        private void InitializeApps()
        {
            var apps = new List<BloatApp>
            {
                new BloatApp { Id = "clipchamp",    Name = "Clipchamp",             Description = "Microsoft's bundled video editor.",                   Category = AppCategory.Microsoft, PackageName = "Clipchamp.Clipchamp",             WinGetId = "Clipchamp.Clipchamp" },
                new BloatApp { Id = "todo",         Name = "Microsoft To Do",        Description = "Task manager app bundled with Windows.",              Category = AppCategory.Microsoft, PackageName = "Microsoft.Todos",                WinGetId = "Microsoft.To-Do" },
                new BloatApp { Id = "bingweather",  Name = "MSN Weather",            Description = "Bing-powered weather app.",                           Category = AppCategory.Microsoft, PackageName = "Microsoft.BingWeather",          WinGetId = "Microsoft.MSNWeather" },
                new BloatApp { Id = "bingnews",     Name = "MSN News",               Description = "Bing-powered news aggregator.",                       Category = AppCategory.Microsoft, PackageName = "Microsoft.BingNews",             WinGetId = "Microsoft.MSNNews" },
                new BloatApp { Id = "feedback",     Name = "Feedback Hub",           Description = "Microsoft feedback collection app.",                  Category = AppCategory.Microsoft, PackageName = "Microsoft.WindowsFeedbackHub",   WinGetId = "Microsoft.WindowsFeedbackHub" },
                new BloatApp { Id = "copilot",      Name = "Copilot",                Description = "Microsoft's AI assistant embedded into Windows.",     Category = AppCategory.AI,        PackageName = "Microsoft.Copilot",              WinGetId = "Microsoft.Copilot" },
                new BloatApp { Id = "paint_ai",     Name = "Paint Cocreator",        Description = "AI image generation built into Paint.",               Category = AppCategory.AI,        PackageName = "Microsoft.Paint",                WinGetId = "Microsoft.Paint" },
                new BloatApp { Id = "xboxapp",      Name = "Xbox App",               Description = "Xbox companion app with background services.",        Category = AppCategory.Games,     PackageName = "Microsoft.GamingApp",            WinGetId = "Microsoft.GamingApp" },
                new BloatApp { Id = "xboxidentity", Name = "Xbox Identity Provider", Description = "Xbox sign-in background service.",                    Category = AppCategory.Games,     PackageName = "Microsoft.XboxIdentityProvider", WinGetId = "Microsoft.XboxIdentityProvider" },
                new BloatApp { Id = "xboxgameui",   Name = "Xbox Game UI",           Description = "Xbox in-game overlay UI component.",                  Category = AppCategory.Games,     PackageName = "Microsoft.XboxGamingOverlay",    WinGetId = "Microsoft.XboxGamingOverlay" },
                new BloatApp { Id = "spotify",      Name = "Spotify",                Description = "Music streaming app.",                                Category = AppCategory.ThirdParty,PackageName = "SpotifyAB.SpotifyMusic",         WinGetId = "Spotify.Spotify" },
            }; 

            foreach (var app in apps)
                AllApps.Add(app);
        }

        private void RefreshAppStates()
        {
            foreach (var app in AllApps)
                app.IsInstalled = PowerShellHelper.IsAppInstalled(app.PackageName);
        }

        [RelayCommand]
        private void GlobalSelect(string param)
        {
            foreach (var tweak in AllTweaks)
            {
                if (param == "Safe") tweak.IsChecked = tweak.Safety == TweakSafety.Safe;
                else if (param == "None") tweak.IsChecked = false;
                else if (param == "All") tweak.IsChecked = true;
            }
        }

        [RelayCommand]
        public void ApplyWithConfirmation()
        {
            if (!AdminHelper.IsRunningAsAdmin())
            {
                DialogManager.CreateDialog()
                    .WithTitle("Administrator Required")
                    .WithContent("Some tweaks require administrator privileges. ReWindows will restart with elevated permissions. Continue?")
                    .OfType(NotificationType.Warning)
                    .WithActionButton("Cancel", _ => { }, true)
                    .WithActionButton("Restart as Admin", _ => AdminHelper.RestartAsAdmin(), true, "Flat", "Accent")
                    .TryShow();
                return;
            }

            var selected = AllTweaks.Where(t => t.IsChecked).ToList();
            var applied = selected.Where(t => t.IsApplied).ToList();
            var notApplied = selected.Where(t => !t.IsApplied).ToList();
            bool hasDangerous = selected.Any(t => t.Safety == TweakSafety.Dangerous);

            if (selected.Count == 0) return;

            if (applied.Count > 0 && notApplied.Count > 0)
            {
                DialogManager.CreateDialog()
                    .WithTitle("Some Tweaks Already Applied")
                    .WithContent($"{applied.Count} of your selected tweaks are already applied. What would you like to do?")
                    .WithActionButton("Cancel", _ => { }, true)
                    .WithActionButton("Apply New Only", _ => ExecuteApply(notApplied), true, "Flat", "Accent")
                    .WithActionButton("Reapply All", _ => ExecuteApply(selected), true, "Flat", "Accent")
                    .TryShow();
            }
            else if (applied.Count > 0 && notApplied.Count == 0)
            {
                DialogManager.CreateDialog()
                    .WithTitle("Already Applied")
                    .WithContent("All selected tweaks are already applied. Do you want to reapply them anyway?")
                    .WithActionButton("Cancel", _ => { }, true)
                    .WithActionButton("Reapply", _ => ExecuteApply(selected), true, "Flat", "Accent")
                    .TryShow();
            }
            else
            {
                DialogManager.CreateDialog()
                    .WithTitle("Confirm Actions")
                    .WithContent(hasDangerous
                        ? "Warning: Some selected tweaks may affect system stability. Proceed?"
                        : "Apply the selected optimizations?")
                    .OfType(hasDangerous ? NotificationType.Warning : NotificationType.Information)
                    .WithActionButton("Cancel", _ => { }, true)
                    .WithActionButton("Apply", _ => ExecuteApply(selected), true, "Flat", "Accent")
                    .TryShow();
            }
        }

        private async void ExecuteApply(List<Tweak> tweaks)
        {
            await Task.Run(() =>
            {
                foreach (var tweak in tweaks)
                {
                    try { tweak.RunAction?.Invoke(); }
                    catch { }
                }
            });

            RefreshTweakStates();
        }

        [RelayCommand]
        public void RevertWithConfirmation()
        {
            var applied = AllTweaks.Where(t => t.IsApplied).ToList();
            if (applied.Count == 0) return;

            DialogManager.CreateDialog()
                .WithTitle("Undo Applied Tweaks")
                .WithContent($"This will revert all {applied.Count} applied tweaks back to Windows defaults. Continue?")
                .WithActionButton("Cancel", _ => { }, true)
                .WithActionButton("Revert", _ => ExecuteRevert(applied), true, "Flat", "Accent")
                .TryShow();
        }

        private async void ExecuteRevert(List<Tweak> tweaks)
        {
            await Task.Run(() =>
            {
                foreach (var tweak in tweaks)
                {
                    try { tweak.RevertAction?.Invoke(); }
                    catch { }
                }
            });

            RefreshTweakStates();
        }

        [RelayCommand]
        public void RemoveAppsWithConfirmation()
        {
            var selected = AllApps.Where(a => a.IsChecked && a.IsInstalled).ToList();
            if (selected.Count == 0) return;

            DialogManager.CreateDialog()
                .WithTitle("Remove Apps")
                .WithContent($"This will uninstall {selected.Count} app(s) from your system. Continue?")
                .WithActionButton("Cancel", _ => { }, true)
                .WithActionButton("Remove", _ => ExecuteRemoveApps(selected), true, "Flat", "Accent")
                .TryShow();
        }

        private async void ExecuteRemoveApps(List<BloatApp> apps)
        {
            await Task.Run(() =>
            {
                foreach (var app in apps)
                {
                    try { PowerShellHelper.RemoveApp(app.PackageName); }
                    catch { }
                }
            });

            RefreshAppStates();
        }

        [RelayCommand]
        public void ReinstallAppsWithConfirmation()
        {
            var selected = AllApps.Where(a => a.IsChecked && !a.IsInstalled).ToList();
            if (selected.Count == 0) return;

            DialogManager.CreateDialog()
                .WithTitle("Reinstall Apps")
                .WithContent($"This will attempt to reinstall {selected.Count} app(s). Continue?")
                .WithActionButton("Cancel", _ => { }, true)
                .WithActionButton("Reinstall", _ => ExecuteReinstallApps(selected), true, "Flat", "Accent")
                .TryShow();
        }

        private async void ExecuteReinstallApps(List<BloatApp> apps)
        {
            await Task.Run(() =>
            {
                foreach (var app in apps)
                {
                    try { PowerShellHelper.ReinstallApp(app.WinGetId); }
                    catch { }
                }
            });

            RefreshAppStates();
        }
    }
}