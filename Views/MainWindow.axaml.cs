using Avalonia.Interactivity;
using ReWindows.ViewModels;
using SukiUI.Controls;
using SukiUI.Enums;

namespace ReWindows.Views
{
    public partial class MainWindow : SukiWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext!;
            vm.Settings.BackgroundStyleChanged += style => BackgroundStyle = style;
            BackgroundStyle = vm.Settings.BackgroundStyle;
            BackgroundAnimationEnabled = true;
        }

        private void Background_OnClick(object? sender, RoutedEventArgs e)
        {
            string? tag = sender switch
            {
                Avalonia.Controls.Button btn => btn.Tag?.ToString(),
                Avalonia.Controls.MenuItem item => item.Tag?.ToString(),
                _ => null
            };

            if (tag == null) return;
            if (!System.Enum.TryParse<SukiBackgroundStyle>(tag, out var style)) return;

            var vm = (MainWindowViewModel)DataContext!;
            var theme = SukiUI.SukiTheme.GetInstance();

            if (theme.ActiveBaseTheme != Avalonia.Styling.ThemeVariant.Dark)
            {
                theme.ChangeBaseTheme(Avalonia.Styling.ThemeVariant.Dark);
                vm.Settings.IsDarkMode = true;
            }

            BackgroundAnimationEnabled = true;
            BackgroundStyle = style;
            vm.Settings.BackgroundStyle = style;
        }
    }
}