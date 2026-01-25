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
        /// 表示モード（タイル/リスト）
        /// </summary>
        public ViewMode ViewMode { get; set; } = ViewMode.Tile;

        /// <summary>
        /// カテゴリ切替方式
        /// </summary>
        public CategoryDisplayMode CategoryDisplayMode { get; set; } = CategoryDisplayMode.TabTop;

        /// <summary>
        /// タイルサイズ
        /// </summary>
        public TileSize TileSize { get; set; } = TileSize.Medium;

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
    }
}
