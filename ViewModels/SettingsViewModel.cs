using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Enums;
using System;

namespace ReWindows.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public Action<SukiBackgroundStyle>? BackgroundStyleChanged { get; set; }

        private readonly SukiTheme _theme = SukiTheme.GetInstance();
        private bool _ready = false;

        [ObservableProperty] private bool _isDarkMode;
        [ObservableProperty] private SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.Gradient;

        public SettingsViewModel()
        {
            var saved = TweakTracker.LoadSettings();

            _isDarkMode = saved.IsDarkMode;
            _theme.ChangeBaseTheme(saved.IsDarkMode ? ThemeVariant.Dark : ThemeVariant.Light);

            if (Enum.TryParse<SukiBackgroundStyle>(saved.BackgroundStyle, out var style))
                _backgroundStyle = style;

            _theme.OnBaseThemeChanged += variant =>
            {
                IsDarkMode = variant == ThemeVariant.Dark;
            };

            _ready = true;
        }

        partial void OnIsDarkModeChanged(bool value)
        {
            _theme.ChangeBaseTheme(value ? ThemeVariant.Dark : ThemeVariant.Light);
            if (_ready) SaveSettings();
        }

        partial void OnBackgroundStyleChanged(SukiBackgroundStyle value)
        {
            BackgroundStyleChanged?.Invoke(value);
            if (_ready) SaveSettings();
        }

        [RelayCommand]
        private void SetBackground(SukiBackgroundStyle style) => BackgroundStyle = style;

        private void SaveSettings()
        {
            TweakTracker.SaveSettings(new AppSettings
            {
                IsDarkMode = IsDarkMode,
                BackgroundStyle = BackgroundStyle.ToString()
            });
        }
    }
}