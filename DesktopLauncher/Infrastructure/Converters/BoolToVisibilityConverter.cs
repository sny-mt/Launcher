using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLauncher.Infrastructure.Converters
{
    /// <summary>
    /// bool値をVisibilityに変換するコンバーター
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // パラメータが"Inverse"の場合は反転
                if (parameter?.ToString() == "Inverse")
                {
                    boolValue = !boolValue;
                }

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                var result = visibility == Visibility.Visible;

                if (parameter?.ToString() == "Inverse")
                {
                    result = !result;
                }

                return result;
            }

            return false;
        }
    }
}
