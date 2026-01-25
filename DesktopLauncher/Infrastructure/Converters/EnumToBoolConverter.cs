using System;
using System.Globalization;
using System.Windows.Data;

namespace DesktopLauncher.Infrastructure.Converters
{
    /// <summary>
    /// Enum値をboolに変換するコンバーター（RadioButton用）
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;

            var enumValue = value.ToString();
            var targetValue = parameter.ToString();

            return enumValue?.Equals(targetValue, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }

            return Binding.DoNothing;
        }
    }
}
