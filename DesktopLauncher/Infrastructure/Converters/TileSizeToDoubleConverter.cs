using System;
using System.Globalization;
using System.Windows.Data;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Infrastructure.Converters
{
    /// <summary>
    /// TileSizeをピクセル値に変換するコンバーター
    /// parameter: "Icon" = アイコンサイズ, "Text" = テキスト幅, それ以外 = タイルサイズ
    /// </summary>
    public class TileSizeToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TileSize tileSize)
            {
                var paramStr = parameter as string ?? "";

                return paramStr switch
                {
                    "Icon" => tileSize switch
                    {
                        TileSize.Small => 24.0,
                        TileSize.Medium => 32.0,
                        TileSize.Large => 48.0,
                        _ => 32.0
                    },
                    "Text" => tileSize switch
                    {
                        TileSize.Small => 56.0,
                        TileSize.Medium => 72.0,
                        TileSize.Large => 92.0,
                        _ => 72.0
                    },
                    "Font" => tileSize switch
                    {
                        TileSize.Small => 8.0,
                        TileSize.Medium => 9.0,
                        TileSize.Large => 10.0,
                        _ => 9.0
                    },
                    "TextHeight" => tileSize switch
                    {
                        TileSize.Small => 24.0,
                        TileSize.Medium => 30.0,
                        TileSize.Large => 36.0,
                        _ => 30.0
                    },
                    _ => tileSize switch
                    {
                        TileSize.Small => 64.0,
                        TileSize.Medium => 80.0,
                        TileSize.Large => 100.0,
                        _ => 80.0
                    }
                };
            }

            return 80.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double size)
            {
                if (size <= 64) return TileSize.Small;
                if (size <= 80) return TileSize.Medium;
                return TileSize.Large;
            }

            return TileSize.Medium;
        }
    }
}
