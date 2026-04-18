using System;
using System.Windows;
using System.Windows.Media;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Services.Ui
{
    /// <summary>
    /// テーマ管理サービスの実装
    /// </summary>
    public class ThemeService : IThemeService
    {
        /// <inheritdoc/>
        public void ApplyTheme(Theme theme)
        {
            var app = Application.Current;
            if (app == null) return;

            // 直接設定したリソース値をクリア
            ClearThemeResourceOverrides(app);

            // ライト系テーマかダーク系テーマかを判定
            var isLightBased = IsLightBasedTheme(theme);
            var baseThemeUri = isLightBased
                ? new Uri("Views/Themes/LightTheme.xaml", UriKind.Relative)
                : new Uri("Views/Themes/DarkTheme.xaml", UriKind.Relative);

            var resources = app.Resources.MergedDictionaries;
            if (resources.Count > 0)
            {
                resources.RemoveAt(0);
            }
            resources.Insert(0, new ResourceDictionary { Source = baseThemeUri });

            // プリセットテーマの場合は追加でカラーを上書き
            if (theme != Theme.Light && theme != Theme.Dark)
            {
                ApplyPresetThemeColors(app, theme);
            }
        }

        /// <inheritdoc/>
        public void ApplyThemeColor(ThemeColor themeColor)
        {
            var app = Application.Current;
            if (app == null) return;

            var (accentColor, accentHoverColor, selectedColor) = GetThemeColors(themeColor);

            app.Resources["AccentColor"] = accentColor;
            app.Resources["AccentHoverColor"] = accentHoverColor;
            app.Resources["SelectedColor"] = selectedColor;
            app.Resources["AccentBrush"] = new SolidColorBrush(accentColor);
            app.Resources["AccentHoverBrush"] = new SolidColorBrush(accentHoverColor);
            app.Resources["SelectedBrush"] = new SolidColorBrush(selectedColor);
        }

        /// <inheritdoc/>
        public void ApplyWindowOpacity(double transparency)
        {
            var app = Application.Current;
            if (app == null) return;

            // 0.0 ~ 0.7 の範囲に制限
            transparency = Math.Max(0.0, Math.Min(0.7, transparency));
            var opacity = 1.0 - transparency;

            // PrimaryColorの透明度を調整
            if (app.Resources["PrimaryColor"] is Color primaryColor)
            {
                var newAlpha = (byte)(opacity * 255);
                var newPrimary = Color.FromArgb(newAlpha, primaryColor.R, primaryColor.G, primaryColor.B);
                app.Resources["PrimaryColor"] = newPrimary;
                app.Resources["PrimaryBrush"] = new SolidColorBrush(newPrimary);
            }

            // SecondaryColorの透明度も調整
            if (app.Resources["SecondaryColor"] is Color secondaryColor)
            {
                var newAlpha = (byte)(Math.Max(0.2, opacity - 0.1) * 255);
                var newSecondary = Color.FromArgb(newAlpha, secondaryColor.R, secondaryColor.G, secondaryColor.B);
                app.Resources["SecondaryColor"] = newSecondary;
                app.Resources["SecondaryBrush"] = new SolidColorBrush(newSecondary);
            }

            // TertiaryColorの透明度も調整
            if (app.Resources["TertiaryColor"] is Color tertiaryColor)
            {
                var newAlpha = (byte)(Math.Max(0.15, opacity - 0.15) * 255);
                var newTertiary = Color.FromArgb(newAlpha, tertiaryColor.R, tertiaryColor.G, tertiaryColor.B);
                app.Resources["TertiaryColor"] = newTertiary;
                app.Resources["TertiaryBrush"] = new SolidColorBrush(newTertiary);
            }
        }

        /// <inheritdoc/>
        public void ApplyCustomTheme(string baseColorHex, string textColorHex, string accentColorHex)
        {
            var app = Application.Current;
            if (app == null) return;

            var baseColor = ParseColor(baseColorHex, Color.FromRgb(30, 30, 30));
            var textColor = ParseColor(textColorHex, Color.FromRgb(255, 255, 255));
            var accentColor = ParseColor(accentColorHex, Color.FromRgb(0, 120, 212));

            // ベースカラーがライトかダークか判定して、適切なベーステーマをロード
            var isLight = IsLightColor(baseColor);
            var baseThemeUri = isLight
                ? new Uri("Views/Themes/LightTheme.xaml", UriKind.Relative)
                : new Uri("Views/Themes/DarkTheme.xaml", UriKind.Relative);

            var resources = app.Resources.MergedDictionaries;
            if (resources.Count > 0)
            {
                resources.RemoveAt(0);
            }
            resources.Insert(0, new ResourceDictionary { Source = baseThemeUri });

            ClearThemeResourceOverrides(app);

            // ベースカラーから派生色を自動算出
            var secondary = ShiftColor(baseColor, isLight ? -10 : 10);
            var tertiary = ShiftColor(baseColor, isLight ? -20 : 20);
            var textSecondary = Color.FromArgb(180, textColor.R, textColor.G, textColor.B);
            var border = Color.FromArgb(100, textColor.R, textColor.G, textColor.B);
            var hover = Color.FromArgb(60, textColor.R, textColor.G, textColor.B);
            var selected = Color.FromArgb(100, accentColor.R, accentColor.G, accentColor.B);
            var accentHover = ShiftColor(accentColor, isLight ? -20 : 20);

            // ベースカラーに透明度を適用（半透明ウィンドウ対応）
            var primary = Color.FromArgb(224, baseColor.R, baseColor.G, baseColor.B);
            var secondaryA = Color.FromArgb(208, secondary.R, secondary.G, secondary.B);
            var tertiaryA = Color.FromArgb(192, tertiary.R, tertiary.G, tertiary.B);

            app.Resources["PrimaryColor"] = primary;
            app.Resources["SecondaryColor"] = secondaryA;
            app.Resources["TertiaryColor"] = tertiaryA;
            app.Resources["TextColor"] = textColor;
            app.Resources["TextSecondaryColor"] = textSecondary;
            app.Resources["BorderColor"] = border;
            app.Resources["HoverColor"] = hover;
            app.Resources["SelectedColor"] = selected;

            app.Resources["PrimaryBrush"] = new SolidColorBrush(primary);
            app.Resources["SecondaryBrush"] = new SolidColorBrush(secondaryA);
            app.Resources["TertiaryBrush"] = new SolidColorBrush(tertiaryA);
            app.Resources["TextBrush"] = new SolidColorBrush(textColor);
            app.Resources["TextSecondaryBrush"] = new SolidColorBrush(textSecondary);
            app.Resources["BorderBrush"] = new SolidColorBrush(border);
            app.Resources["HoverBrush"] = new SolidColorBrush(hover);
            app.Resources["SelectedBrush"] = new SolidColorBrush(selected);

            // アクセントカラーも適用
            app.Resources["AccentColor"] = accentColor;
            app.Resources["AccentHoverColor"] = accentHover;
            app.Resources["AccentBrush"] = new SolidColorBrush(accentColor);
            app.Resources["AccentHoverBrush"] = new SolidColorBrush(accentHover);

            // ダイアログ用（不透明）
            var dialogPrimary = Color.FromArgb(255, baseColor.R, baseColor.G, baseColor.B);
            var dialogSecondary = Color.FromArgb(255, secondary.R, secondary.G, secondary.B);
            app.Resources["DialogPrimaryColor"] = dialogPrimary;
            app.Resources["DialogSecondaryColor"] = dialogSecondary;
            app.Resources["DialogPrimaryBrush"] = new SolidColorBrush(dialogPrimary);
            app.Resources["DialogSecondaryBrush"] = new SolidColorBrush(dialogSecondary);
        }

        private static Color ParseColor(string hex, Color fallback)
        {
            try
            {
                var obj = ColorConverter.ConvertFromString(hex);
                return obj is Color c ? c : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static bool IsLightColor(Color color)
        {
            // 相対輝度で判定
            var luminance = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
            return luminance > 128;
        }

        private static Color ShiftColor(Color color, int amount)
        {
            return Color.FromRgb(
                (byte)Math.Max(0, Math.Min(255, color.R + amount)),
                (byte)Math.Max(0, Math.Min(255, color.G + amount)),
                (byte)Math.Max(0, Math.Min(255, color.B + amount)));
        }

        /// <inheritdoc/>
        public bool IsLightBasedTheme(Theme theme)
        {
            return theme switch
            {
                Theme.Light => true,
                Theme.Sakura => true,
                Theme.Lavender => true,
                Theme.Candy => true,
                Theme.Peach => true,
                Theme.Milky => true,
                Theme.Dreamy => true,
                Theme.Custom => false, // Custom時はApplyCustomTheme内で判定
                _ => false
            };
        }

        private void ClearThemeResourceOverrides(Application app)
        {
            string[] keysToRemove = {
                "PrimaryColor", "SecondaryColor", "TertiaryColor",
                "TextColor", "TextSecondaryColor", "BorderColor",
                "HoverColor", "SelectedColor",
                "PrimaryBrush", "SecondaryBrush", "TertiaryBrush",
                "TextBrush", "TextSecondaryBrush", "BorderBrush",
                "HoverBrush", "SelectedBrush",
                "DialogPrimaryColor", "DialogSecondaryColor",
                "DialogPrimaryBrush", "DialogSecondaryBrush",
                "AccentColor", "AccentHoverColor",
                "AccentBrush", "AccentHoverBrush"
            };

            foreach (var key in keysToRemove)
            {
                if (app.Resources.Contains(key))
                {
                    app.Resources.Remove(key);
                }
            }
        }

        private void ApplyPresetThemeColors(Application app, Theme theme)
        {
            var (primary, secondary, tertiary, text, textSecondary, border, hover, selected) = GetPresetThemeColors(theme);

            app.Resources["PrimaryColor"] = primary;
            app.Resources["SecondaryColor"] = secondary;
            app.Resources["TertiaryColor"] = tertiary;
            app.Resources["TextColor"] = text;
            app.Resources["TextSecondaryColor"] = textSecondary;
            app.Resources["BorderColor"] = border;
            app.Resources["HoverColor"] = hover;
            app.Resources["SelectedColor"] = selected;

            app.Resources["PrimaryBrush"] = new SolidColorBrush(primary);
            app.Resources["SecondaryBrush"] = new SolidColorBrush(secondary);
            app.Resources["TertiaryBrush"] = new SolidColorBrush(tertiary);
            app.Resources["TextBrush"] = new SolidColorBrush(text);
            app.Resources["TextSecondaryBrush"] = new SolidColorBrush(textSecondary);
            app.Resources["BorderBrush"] = new SolidColorBrush(border);
            app.Resources["HoverBrush"] = new SolidColorBrush(hover);
            app.Resources["SelectedBrush"] = new SolidColorBrush(selected);
        }

        private (Color primary, Color secondary, Color tertiary, Color text, Color textSecondary, Color border, Color hover, Color selected) GetPresetThemeColors(Theme theme)
        {
            return theme switch
            {
                Theme.Midnight => (
                    Color.FromArgb(224, 15, 23, 42),
                    Color.FromArgb(208, 30, 41, 59),
                    Color.FromArgb(192, 51, 65, 85),
                    Color.FromRgb(226, 232, 240),
                    Color.FromArgb(204, 148, 163, 184),
                    Color.FromArgb(128, 71, 85, 105),
                    Color.FromArgb(96, 51, 65, 85),
                    Color.FromArgb(128, 59, 130, 246)
                ),
                Theme.Sepia => (
                    Color.FromArgb(224, 68, 55, 43),
                    Color.FromArgb(208, 87, 71, 56),
                    Color.FromArgb(192, 107, 89, 72),
                    Color.FromRgb(255, 248, 235),
                    Color.FromArgb(204, 215, 195, 170),
                    Color.FromArgb(128, 139, 115, 90),
                    Color.FromArgb(96, 107, 89, 72),
                    Color.FromArgb(128, 180, 130, 80)
                ),
                Theme.Forest => (
                    Color.FromArgb(224, 20, 45, 35),
                    Color.FromArgb(208, 32, 60, 48),
                    Color.FromArgb(192, 45, 75, 60),
                    Color.FromRgb(220, 240, 230),
                    Color.FromArgb(204, 150, 180, 165),
                    Color.FromArgb(128, 60, 100, 80),
                    Color.FromArgb(96, 45, 75, 60),
                    Color.FromArgb(128, 34, 139, 34)
                ),
                Theme.Ocean => (
                    Color.FromArgb(224, 15, 45, 75),
                    Color.FromArgb(208, 25, 60, 95),
                    Color.FromArgb(192, 35, 75, 115),
                    Color.FromRgb(224, 240, 255),
                    Color.FromArgb(204, 140, 175, 210),
                    Color.FromArgb(128, 50, 100, 150),
                    Color.FromArgb(96, 35, 75, 115),
                    Color.FromArgb(128, 0, 150, 200)
                ),
                Theme.Rose => (
                    Color.FromArgb(224, 65, 35, 50),
                    Color.FromArgb(208, 85, 45, 65),
                    Color.FromArgb(192, 105, 55, 80),
                    Color.FromRgb(255, 235, 240),
                    Color.FromArgb(204, 200, 160, 175),
                    Color.FromArgb(128, 130, 70, 95),
                    Color.FromArgb(96, 105, 55, 80),
                    Color.FromArgb(128, 220, 100, 140)
                ),
                Theme.Slate => (
                    Color.FromArgb(224, 45, 50, 55),
                    Color.FromArgb(208, 60, 65, 72),
                    Color.FromArgb(192, 75, 80, 88),
                    Color.FromRgb(235, 238, 242),
                    Color.FromArgb(204, 160, 168, 178),
                    Color.FromArgb(128, 90, 95, 105),
                    Color.FromArgb(96, 75, 80, 88),
                    Color.FromArgb(128, 100, 116, 139)
                ),
                Theme.Sakura => (
                    Color.FromArgb(230, 255, 240, 245),
                    Color.FromArgb(220, 255, 228, 235),
                    Color.FromArgb(210, 255, 218, 228),
                    Color.FromRgb(80, 50, 60),
                    Color.FromArgb(180, 140, 100, 115),
                    Color.FromArgb(100, 255, 182, 193),
                    Color.FromArgb(80, 255, 192, 203),
                    Color.FromArgb(100, 255, 105, 180)
                ),
                Theme.Lavender => (
                    Color.FromArgb(230, 245, 240, 255),
                    Color.FromArgb(220, 238, 230, 255),
                    Color.FromArgb(210, 230, 220, 250),
                    Color.FromRgb(60, 50, 80),
                    Color.FromArgb(180, 120, 100, 150),
                    Color.FromArgb(100, 200, 180, 230),
                    Color.FromArgb(80, 210, 190, 240),
                    Color.FromArgb(100, 147, 112, 219)
                ),
                Theme.Candy => (
                    Color.FromArgb(230, 255, 245, 250),
                    Color.FromArgb(220, 255, 235, 245),
                    Color.FromArgb(210, 255, 225, 240),
                    Color.FromRgb(100, 60, 80),
                    Color.FromArgb(180, 160, 100, 130),
                    Color.FromArgb(100, 255, 150, 200),
                    Color.FromArgb(80, 255, 180, 210),
                    Color.FromArgb(100, 255, 110, 180)
                ),
                Theme.Peach => (
                    Color.FromArgb(230, 255, 245, 238),
                    Color.FromArgb(220, 255, 235, 225),
                    Color.FromArgb(210, 255, 225, 210),
                    Color.FromRgb(100, 60, 50),
                    Color.FromArgb(180, 160, 110, 95),
                    Color.FromArgb(100, 255, 200, 180),
                    Color.FromArgb(80, 255, 210, 190),
                    Color.FromArgb(100, 255, 140, 105)
                ),
                Theme.Milky => (
                    Color.FromArgb(235, 255, 253, 250),
                    Color.FromArgb(225, 255, 250, 245),
                    Color.FromArgb(215, 250, 245, 240),
                    Color.FromRgb(90, 75, 70),
                    Color.FromArgb(170, 140, 125, 118),
                    Color.FromArgb(90, 220, 210, 200),
                    Color.FromArgb(70, 240, 230, 220),
                    Color.FromArgb(90, 200, 180, 170)
                ),
                Theme.Dreamy => (
                    Color.FromArgb(225, 240, 235, 255),
                    Color.FromArgb(215, 250, 240, 255),
                    Color.FromArgb(205, 255, 245, 255),
                    Color.FromRgb(80, 60, 100),
                    Color.FromArgb(170, 140, 120, 170),
                    Color.FromArgb(90, 220, 200, 255),
                    Color.FromArgb(70, 240, 220, 255),
                    Color.FromArgb(90, 200, 150, 255)
                ),
                _ => (
                    Color.FromArgb(224, 30, 30, 30),
                    Color.FromArgb(208, 37, 37, 38),
                    Color.FromArgb(192, 45, 45, 48),
                    Color.FromRgb(255, 255, 255),
                    Color.FromArgb(204, 204, 204, 204),
                    Color.FromArgb(128, 63, 63, 70),
                    Color.FromArgb(96, 62, 62, 66),
                    Color.FromArgb(128, 9, 71, 113)
                )
            };
        }

        private (Color accent, Color hover, Color selected) GetThemeColors(ThemeColor themeColor)
        {
            return themeColor switch
            {
                ThemeColor.Blue => (
                    Color.FromRgb(0, 120, 212),
                    Color.FromRgb(16, 110, 190),
                    Color.FromArgb(128, 9, 71, 113)
                ),
                ThemeColor.Pink => (
                    Color.FromRgb(227, 0, 140),
                    Color.FromRgb(194, 0, 120),
                    Color.FromArgb(128, 140, 0, 86)
                ),
                ThemeColor.Green => (
                    Color.FromRgb(16, 137, 62),
                    Color.FromRgb(14, 119, 54),
                    Color.FromArgb(128, 10, 82, 38)
                ),
                ThemeColor.Red => (
                    Color.FromRgb(232, 17, 35),
                    Color.FromRgb(200, 15, 30),
                    Color.FromArgb(128, 140, 10, 21)
                ),
                ThemeColor.Purple => (
                    Color.FromRgb(136, 23, 152),
                    Color.FromRgb(116, 20, 130),
                    Color.FromArgb(128, 82, 14, 91)
                ),
                ThemeColor.Orange => (
                    Color.FromRgb(255, 140, 0),
                    Color.FromRgb(224, 123, 0),
                    Color.FromArgb(128, 153, 84, 0)
                ),
                ThemeColor.Passion => (
                    Color.FromRgb(218, 59, 59),
                    Color.FromRgb(190, 51, 51),
                    Color.FromArgb(128, 131, 35, 35)
                ),
                ThemeColor.Cutie => (
                    Color.FromRgb(255, 105, 180),
                    Color.FromRgb(255, 20, 147),
                    Color.FromArgb(128, 153, 63, 108)
                ),
                ThemeColor.Mint => (
                    Color.FromRgb(0, 177, 169),
                    Color.FromRgb(0, 153, 146),
                    Color.FromArgb(128, 0, 106, 101)
                ),
                ThemeColor.Sunset => (
                    Color.FromRgb(255, 99, 71),
                    Color.FromRgb(255, 69, 0),
                    Color.FromArgb(128, 153, 59, 43)
                ),
                _ => (
                    Color.FromRgb(0, 120, 212),
                    Color.FromRgb(16, 110, 190),
                    Color.FromArgb(128, 9, 71, 113)
                )
            };
        }
    }
}
