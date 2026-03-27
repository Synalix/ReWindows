using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ReWindows
{
    public class InstalledColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && b
                ? new SolidColorBrush(Color.Parse("#c0392b"))
                : new SolidColorBrush(Color.Parse("#3a9e5f"));

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class InstalledTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is bool b && b ? "Installed" : "Not Installed";

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}