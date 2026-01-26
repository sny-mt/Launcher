using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLauncher.Infrastructure.Converters
{
    /// <summary>
    /// Enum値をVisibilityに変換するコンバーター
    /// parameterで指定した値と一致すればVisible、それ以外はCollapsed
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;

            var enumValue = value.ToString();
            var targetValue = parameter.ToString();

            return enumValue?.Equals(targetValue, StringComparison.OrdinalIgnoreCase) == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
