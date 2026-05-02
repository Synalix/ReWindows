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
        }

        private IEnumerable<Tweak> FilterTweaks(string category)
        {
            return AllTweaks.Where(t =>
                t.Category == category &&
                (string.IsNullOrEmpty(SearchText) ||
                 t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                 t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
        }

        private IEnumerable<BloatApp> FilterApps(AppCategory category)
        {
            return AllApps.Where(a =>
                a.Category == category &&
                (string.IsNullOrEmpty(SearchText) ||
                 a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                 a.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
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
                    RunAction    = () => RegistryHelper.SetString(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "0"),
                    RevertAction = () => RegistryHelper.SetString(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "1"),
                    CheckAction  = () => RegistryHelper.GetString(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "1") == "0" },

                new Tweak { Id = "startupdelay", Name = "Disable Startup Delay", Description = "Lets startup apps launch immediately instead of waiting for Windows to settle.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 1) == 0 },

                new Tweak { Id = "vbs", Name = "Disable VBS", Description = "Turns off virtualization-based security for lower overhead on supported PCs.", Category = "Performance", Safety = TweakSafety.Dangerous,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\DeviceGuard", "EnableVirtualizationBasedSecurity", 1) == 0 },

                new Tweak { Id = "hibernation", Name = "Disable Hibernation", Description = "Frees up disk space by removing the hibernation file.", Category = "Performance", Safety = TweakSafety.Moderate,
                    RunAction    = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0);
                        if (RegistryHelper.KeyExists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings"))
                            RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", 0);
                    },
                    RevertAction = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1);
                        if (RegistryHelper.KeyExists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings"))
                            RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", 1);
                    },
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1) == 0 &&
                                       (!RegistryHelper.KeyExists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings") ||
                                        RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowHibernateOption", 1) == 0) },

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

                new Tweak { Id = "copilot", Name = "Disable Copilot + Recall", Description = "Turns off Copilot, hides the button, and blocks Recall-style AI data analysis.", Category = "Privacy", Safety = TweakSafety.Moderate,
                    RunAction    = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1);
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1);
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1);
                    },
                    RevertAction = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 0);
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 0);
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 0);
                    },
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 0) == 1 &&
                                       RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 0) == 1 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 0) == 1 &&
                                       RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 0) == 1 },

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

                new Tweak { Id = "widgets", Name = "Disable Widgets", Description = "Removes the Widgets button from the taskbar and stops its background feed.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0);
                    },
                    RevertAction = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 1);
                    },
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 1) == 0 },

                new Tweak { Id = "stickykeys", Name = "Disable Sticky Keys Prompt", Description = "Stops the Sticky Keys dialog from appearing when Shift is pressed 5 times.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetString(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "506"),
                    RevertAction = () => RegistryHelper.SetString(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "510"),
                    CheckAction  = () => RegistryHelper.GetString(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "510") == "506" },

                new Tweak { Id = "windowstips", Name = "Disable Windows Tips", Description = "Stops Windows from showing tips, ads, and recommendations across Start and the lock screen.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-314559Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0);
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", 0);
                    },
                    RevertAction = () =>
                    {
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-314559Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 1);
                        RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", 1);
                    },
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-314559Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 1) == 0 &&
                                       RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", 1) == 0 },

                new Tweak { Id = "lockscreenads", Name = "Disable Lock Screen Ads", Description = "Prevents Windows Spotlight from showing ads and suggestions on the lock screen.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1) == 0 },

                new Tweak { Id = "autoplay", Name = "Disable AutoPlay", Description = "Stops Windows from automatically running programs when a USB drive or disc is inserted.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 255),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 145),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 145) == 255 },

                new Tweak { Id = "numlock", Name = "NumLock on at Startup", Description = "Enables NumLock automatically when Windows starts.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetString(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "InitialKeyboardIndicators", "2"),
                    RevertAction = () => RegistryHelper.SetString(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "InitialKeyboardIndicators", "0"),
                    CheckAction  = () => RegistryHelper.GetString(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "InitialKeyboardIndicators", "0") == "2" },

                new Tweak { Id = "noautoreboot", Name = "Prevent Update Auto-Reboot", Description = "Stops Windows from automatically restarting to apply updates while you are logged in.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 0) == 1 },

                new Tweak { Id = "backgroundapps", Name = "Disable Background Apps", Description = "Prevents UWP apps from running or updating in the background.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 0) == 1 },

                new Tweak { Id = "remoteassistance", Name = "Disable Remote Assistance", Description = "Prevents others from connecting to your PC via Windows Remote Assistance.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Remote Assistance", "fAllowToGetHelp", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Remote Assistance", "fAllowToGetHelp", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Remote Assistance", "fAllowToGetHelp", 1) == 0 },

                new Tweak { Id = "cloudclipboard", Name = "Disable Cloud Clipboard Sync", Description = "Stops clipboard content from being synced to Microsoft's cloud across your devices.", Category = "Privacy", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableCloudClipboard", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableCloudClipboard", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Clipboard", "EnableCloudClipboard", 1) == 0 },

                new Tweak { Id = "sysmain", Name = "Disable SysMain", Description = "Stops the Superfetch service that preloads apps into RAM. Recommended on SSDs.", Category = "Performance", Safety = TweakSafety.Moderate,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SysMain", "Start", 4),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SysMain", "Start", 2),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SysMain", "Start", 2) == 4 },

                new Tweak { Id = "searchindex", Name = "Disable Search Indexing", Description = "Stops Windows from indexing files in the background. Reduces disk usage, especially on HDDs.", Category = "Performance", Safety = TweakSafety.Moderate,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSearch", "Start", 4),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSearch", "Start", 2),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WSearch", "Start", 2) == 4 },

                new Tweak { Id = "deliveryopt", Name = "Disable Delivery Optimization", Description = "Prevents Windows from using your bandwidth to upload updates to other PCs on the internet.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 1) == 0 },

                new Tweak { Id = "onedrive", Name = "Disable OneDrive", Description = "Stops OneDrive file sync and its background integration.", Category = "Performance", Safety = TweakSafety.Moderate,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", 0) == 1 },

                new Tweak { Id = "fileexplorerads", Name = "Disable File Explorer Ads", Description = "Turns off sync provider and cloud promo banners in File Explorer.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 1),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 1) == 0 },

                new Tweak { Id = "autoexplore", Name = "Disable Explorer Auto Discovery", Description = "Stops Explorer from reclassifying folders and changing their view types.", Category = "System", Safety = TweakSafety.Moderate,
                    RunAction    = () =>
                    {
                        RegistryHelper.DeleteKey(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags");
                        RegistryHelper.DeleteKey(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU");
                    },
                    RevertAction = () =>
                    {
                        RegistryHelper.CreateKey(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags");
                        RegistryHelper.CreateKey(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU");
                    },
                    CheckAction  = () => !RegistryHelper.KeyExists(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags") &&
                                       !RegistryHelper.KeyExists(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU") },

                new Tweak { Id = "explorerhomegallery", Name = "Remove Explorer Home & Gallery", Description = "Removes Home and Gallery from Explorer and opens This PC by default.", Category = "System", Safety = TweakSafety.Moderate,
                    RunAction    = () =>
                    {
                        RegistryHelper.DeleteKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}");
                        RegistryHelper.DeleteKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1);
                    },
                    RevertAction = () =>
                    {
                        RegistryHelper.CreateKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}");
                        RegistryHelper.CreateKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                        RegistryHelper.SetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 2);
                    },
                    CheckAction  = () => !RegistryHelper.KeyExists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}") &&
                                       !RegistryHelper.KeyExists(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}") &&
                                       RegistryHelper.GetDword(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 2) == 1 },

                new Tweak { Id = "classiccontextmenu", Name = "Set Classic Right-Click Menu", Description = "Restores the classic Explorer context menu in Windows 11.", Category = "System", Safety = TweakSafety.Safe,
                    RunAction    = () =>
                    {
                        const string keyPath = @"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";
                        RegistryHelper.CreateKey(keyPath);
                        RegistryHelper.SetString(keyPath, "", "");
                    },
                    RevertAction = () => RegistryHelper.DeleteKey(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"),
                    CheckAction  = () => RegistryHelper.KeyExists(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32") &&
                                       string.IsNullOrEmpty(RegistryHelper.GetStringOrNull(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "")) },

                new Tweak { Id = "excludedriverupdates", Name = "Exclude Driver Updates", Description = "Stops Windows Update from pushing driver updates automatically.", Category = "Performance", Safety = TweakSafety.Safe,
                    RunAction    = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1),
                    RevertAction = () => RegistryHelper.SetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 0),
                    CheckAction  = () => RegistryHelper.GetDword(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 0) == 1 },
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
                new BloatApp { Id = "clipchamp",    Name = "Clipchamp",             Description = "Microsoft's bundled video editor.",                   Category = AppCategory.Microsoft, PackageName = "Clipchamp.Clipchamp",             WinGetId = "9P1J8S7CCWWT",           WingetSource = "msstore" },
                new BloatApp { Id = "todo",         Name = "Microsoft To Do",        Description = "Task manager app bundled with Windows.",              Category = AppCategory.Microsoft, PackageName = "Microsoft.Todos",                WinGetId = "9NBLGGH5R558",           WingetSource = "msstore" },
                new BloatApp { Id = "bingweather",  Name = "MSN Weather",            Description = "Bing-powered weather app.",                           Category = AppCategory.Microsoft, PackageName = "Microsoft.BingWeather",          WinGetId = "9WZDNCRFJ3Q2",           WingetSource = "msstore" },
                new BloatApp { Id = "bingnews",     Name = "Microsoft News",         Description = "Bing-powered news feed.",                             Category = AppCategory.Microsoft, PackageName = "Microsoft.BingNews",             WinGetId = "9WZDNCRFHVFW",           WingetSource = "msstore" },
                new BloatApp { Id = "feedback",     Name = "Feedback Hub",           Description = "Microsoft feedback collection app.",                  Category = AppCategory.Microsoft, PackageName = "Microsoft.WindowsFeedbackHub",   WinGetId = "9NBLGGH4R32N",           WingetSource = "msstore" },
                new BloatApp { Id = "teams",        Name = "Microsoft Teams",        Description = "Microsoft's consumer chat and meeting app.",         Category = AppCategory.Microsoft, PackageName = "MSTeams",                       WinGetId = "XP8BT8DW290MPQ",         WingetSource = "msstore" },
                new BloatApp { Id = "onenote",      Name = "OneNote",               Description = "Microsoft's note-taking app.",                       Category = AppCategory.Microsoft, PackageName = "Microsoft.Office.OneNote",      WinGetId = "XPFFZHVGQWWLHB",         WingetSource = "msstore" },
                new BloatApp { Id = "mixedreality", Name = "Mixed Reality Portal",   Description = "Windows Mixed Reality setup and launcher app.",      Category = AppCategory.Microsoft, PackageName = "Microsoft.MixedReality.Portal", WinGetId = "9NG1H8B3ZC7M",         WingetSource = "msstore" },
                new BloatApp { Id = "journal",      Name = "Microsoft Journal",      Description = "Pen-focused note app that most users never need.",    Category = AppCategory.Microsoft, PackageName = "Microsoft.MicrosoftJournal",    WinGetId = "9N318R854RHH",           WingetSource = "msstore" },
                new BloatApp { Id = "stickynotes",   Name = "Sticky Notes",           Description = "Simple cloud-synced note app bundled with Windows.",  Category = AppCategory.Microsoft, PackageName = "Microsoft.MicrosoftStickyNotes", WinGetId = "9NBLGGH4QGHW",         WingetSource = "msstore" },
                new BloatApp { Id = "viewer3d",     Name = "3D Viewer",             Description = "Legacy 3D model viewer that most users do not need.", Category = AppCategory.Microsoft, PackageName = "Microsoft.Microsoft3DViewer",   WinGetId = "9NBLGGH42THS",         WingetSource = "msstore" },
                new BloatApp { Id = "outlook",      Name = "Outlook for Windows",   Description = "Microsoft's consumer email app.",                    Category = AppCategory.Microsoft, PackageName = "Microsoft.OutlookForWindows",  WinGetId = "9NRX63209R7B",           WingetSource = "msstore" },
                new BloatApp { Id = "whiteboard",   Name = "Microsoft Whiteboard",  Description = "Microsoft's collaborative whiteboard app.",         Category = AppCategory.Microsoft, PackageName = "Microsoft.Whiteboard",           WinGetId = "9MSPC6MP8FM4",         WingetSource = "msstore" },
                new BloatApp { Id = "maps",         Name = "Windows Maps",          Description = "Built-in map and location app.",                     Category = AppCategory.Microsoft, PackageName = "Microsoft.WindowsMaps",         WinGetId = "9NBLGGH6JZ60",         WingetSource = "msstore" },
                new BloatApp { Id = "soundrecorder", Name = "Sound Recorder",         Description = "Windows voice recording app.",                       Category = AppCategory.Microsoft, PackageName = "Microsoft.WindowsSoundRecorder", WinGetId = "9WZDNCRFHWKN",        WingetSource = "msstore" },
                new BloatApp { Id = "copilot",      Name = "Copilot",                Description = "Microsoft's AI assistant embedded into Windows.",     Category = AppCategory.AI,        PackageName = "Microsoft.Copilot",              WinGetId = "XP9CXNGPPJ97XX",        WingetSource = "msstore" },
                new BloatApp { Id = "paint_ai",     Name = "Paint Cocreator",        Description = "AI image generation built into Paint.",               Category = AppCategory.AI,        PackageName = "Microsoft.Paint",                WinGetId = "9PCFS5B6T72H",         WingetSource = "msstore" },
                new BloatApp { Id = "xboxapp",      Name = "Xbox App",               Description = "Xbox companion app with background services.",        Category = AppCategory.Games,     PackageName = "Microsoft.GamingApp",            WinGetId = "9MV0B5HZVK9Z",         WingetSource = "msstore" },
                new BloatApp { Id = "spotify",      Name = "Spotify",                Description = "Music streaming app.",                                Category = AppCategory.ThirdParty,PackageName = "SpotifyAB.SpotifyMusic",         WinGetId = "Spotify.Spotify", WingetSource = "winget" },
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

            foreach (var app in AllApps)
            {
                if (param == "None") app.IsChecked = false;
                else if (param == "All") app.IsChecked = true;
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
            var failed = new List<string>();

            await Task.Run(() =>
            {
                foreach (var tweak in tweaks)
                {
                    try { tweak.RunAction?.Invoke(); }
                    catch (Exception ex) when (ex is not StackOverflowException and not OutOfMemoryException)
                    { failed.Add(tweak.Name); }
                }
            });

            RefreshTweakStates();

            if (failed.Count > 0)
            {
                DialogManager.CreateDialog()
                    .WithTitle("Some Tweaks Failed")
                    .WithContent($"Failed to apply: {string.Join(", ", failed)}\n\nThis may require administrator privileges.")
                    .OfType(NotificationType.Warning)
                    .WithActionButton("OK", _ => { }, true)
                    .TryShow();
            }
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
            var failed = new List<string>();

            await Task.Run(() =>
            {
                foreach (var tweak in tweaks)
                {
                    try { tweak.RevertAction?.Invoke(); }
                    catch (Exception ex) when (ex is not StackOverflowException and not OutOfMemoryException)
                    { failed.Add(tweak.Name); }
                }
            });

            RefreshTweakStates();

            if (failed.Count > 0)
            {
                DialogManager.CreateDialog()
                    .WithTitle("Some Tweaks Failed")
                    .WithContent($"Failed to revert: {string.Join(", ", failed)}\n\nThis may require administrator privileges.")
                    .OfType(NotificationType.Warning)
                    .WithActionButton("OK", _ => { }, true)
                    .TryShow();
            }
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
            int successCount = 0;
            var failures = new List<string>();

            await Task.Run(() =>
            {
                foreach (var app in apps)
                {
                    var result = PowerShellHelper.RemoveApp(app.PackageName);
                    if (result.Success)
                    {
                        successCount++;
                        continue;
                    }

                    failures.Add($"{app.Name}: {GetSummaryMessage(result)}");
                }
            });

            RefreshAppStates();
            ShowAppActionSummary("Remove Apps", "removed", successCount, failures);
        }

        [RelayCommand]
        public void ReinstallAppsWithConfirmation()
        {
            var selected = AllApps.Where(a => a.IsChecked && !a.IsInstalled).ToList();
            if (selected.Count == 0) return;

            DialogManager.CreateDialog()
                .WithTitle("Restore Apps")
                .WithContent($"This will attempt to reinstall {selected.Count} app(s). Continue?")
                .WithActionButton("Cancel", _ => { }, true)
                .WithActionButton("Restore", _ => ExecuteReinstallApps(selected), true, "Flat", "Accent")
                .TryShow();
        }

        private async void ExecuteReinstallApps(List<BloatApp> apps)
        {
            int successCount = 0;
            var failures = new List<string>();

            await Task.Run(() =>
            {
                foreach (var app in apps)
                {
                    var result = PowerShellHelper.ReinstallApp(app.WinGetId, app.WingetSource);
                    if (result.Success)
                    {
                        successCount++;
                        continue;
                    }

                    failures.Add($"{app.Name}: {GetSummaryMessage(result)}");
                }
            });

            RefreshAppStates();
            ShowAppActionSummary("Restore Apps", "restored", successCount, failures);
        }

        private void ShowAppActionSummary(string title, string actionVerb, int successCount, List<string> failures)
        {
            string content = successCount > 0
                ? $"{successCount} app(s) {actionVerb}."
                : $"No apps {actionVerb}.";

            if (failures.Count > 0)
                content += $"\n\nFailed:\n- {string.Join("\n- ", failures)}";

            DialogManager.CreateDialog()
                .WithTitle(title)
                .WithContent(content)
                .OfType(failures.Count > 0 ? NotificationType.Warning : NotificationType.Information)
                .WithActionButton("OK", _ => { }, true)
                .TryShow();
        }

        private static string GetSummaryMessage(CommandResult result)
        {
            string message = result.CombinedOutput.Trim();
            if (string.IsNullOrWhiteSpace(message))
                message = result.StandardError.Trim();

            if (string.IsNullOrWhiteSpace(message))
                message = $"Exit code {result.ExitCode}.";

            int newlineIndex = message.IndexOf('\n');
            if (newlineIndex >= 0)
                message = message[..newlineIndex].Trim();

            return message;
        }
    }
}
