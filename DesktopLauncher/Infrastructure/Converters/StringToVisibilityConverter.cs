using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DesktopLauncher.Infrastructure.Converters
{
    /// <summary>
    /// 文字列をVisibilityに変換するコンバーター
    /// 文字列がある場合はVisible、空の場合はCollapsed
    /// ConverterParameter=Invertで反転
    /// ConverterParameter=特定の文字列 で値と一致する場合Visible
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            var param = parameter as string;

            // パラメータが "Invert" の場合は従来の反転ロジック
            if (param == "Invert")
            {
                var hasText = !string.IsNullOrWhiteSpace(text);
                return hasText ? Visibility.Collapsed : Visibility.Visible;
            }

            // パラメータが指定されている場合は値と比較
            if (!string.IsNullOrEmpty(param))
            {
                return text == param ? Visibility.Visible : Visibility.Collapsed;
            }

            // パラメータなしの場合は文字列があるかどうか
            var hasValue = !string.IsNullOrWhiteSpace(text);
            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
