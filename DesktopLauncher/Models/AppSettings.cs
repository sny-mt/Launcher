using System.Windows.Input;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Models
{
    /// <summary>
    /// アプリケーション設定
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// ホットキー - 修飾キー（Alt, Ctrl, Shift, Win）
        /// </summary>
        public ModifierKeys HotkeyModifiers { get; set; } = ModifierKeys.Alt;

        /// <summary>
        /// ホットキー - キー
        /// </summary>
        public Key HotkeyKey { get; set; } = Key.Space;

        /// <summary>
        /// テーマ
        /// </summary>
        public Theme Theme { get; set; } = Theme.Dark;

        /// <summary>
        /// テーマカラー
        /// </summary>
        public ThemeColor ThemeColor { get; set; } = ThemeColor.Blue;

        /// <summary>
        /// ウィンドウの透明度 (0.0=不透明 - 0.7=透明)
        /// </summary>
        public double WindowOpacity { get; set; } = 0.1;

        /// <summary>
        /// タイルサイズ
        /// </summary>
        public TileSize TileSize { get; set; } = TileSize.Medium;

        /// <summary>
        /// フォントサイズ (8-16)
        /// </summary>
        public double FontSize { get; set; } = 11.0;

        /// <summary>
        /// フォント名
        /// </summary>
        public string FontFamily { get; set; } = "Yu Gothic UI";

        /// <summary>
        /// アイコンサイズ (24-64)
        /// </summary>
        public double IconSize { get; set; } = 32.0;

        /// <summary>
        /// Windows起動時に自動起動
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// 起動時に最小化
        /// </summary>
        public bool StartMinimized { get; set; } = true;

        /// <summary>
        /// アイテム起動後にウィンドウを隠す
        /// </summary>
        public bool HideAfterLaunch { get; set; } = true;

        /// <summary>
        /// カスタムテーマ - ベースカラー（背景色）HEX形式
        /// </summary>
        public string CustomBaseColor { get; set; } = "#1E1E1E";

        /// <summary>
        /// カスタムテーマ - テキストカラー HEX形式
        /// </summary>
        public string CustomTextColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// カスタムテーマ - アクセントカラー HEX形式
        /// </summary>
        public string CustomAccentColor { get; set; } = "#0078D4";
    }
}
