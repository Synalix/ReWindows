using Avalonia.Data.Converters;
using Avalonia.Media;
using ReWindows.ViewModels;
using System;
using System.Globalization;

namespace ReWindows
{
    public class SafetyColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is TweakSafety safety ? safety switch
            {
                TweakSafety.Safe => new SolidColorBrush(Color.Parse("#3a9e5f")),
                TweakSafety.Moderate => new SolidColorBrush(Color.Parse("#d4812a")),
                TweakSafety.Dangerous => new SolidColorBrush(Color.Parse("#c0392b")),
                _ => Brushes.Gray
            } : Brushes.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}