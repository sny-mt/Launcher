using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Interfaces.Services
{
    /// <summary>
    /// テーマ管理サービスのインターフェース
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// テーマを適用する
        /// </summary>
        /// <param name="theme">適用するテーマ</param>
        void ApplyTheme(Theme theme);

        /// <summary>
        /// テーマカラー（アクセントカラー）を適用する
        /// </summary>
        /// <param name="themeColor">適用するテーマカラー</param>
        void ApplyThemeColor(ThemeColor themeColor);

        /// <summary>
        /// ウィンドウの透明度を適用する
        /// </summary>
        /// <param name="transparency">透明度（0.0～0.7）</param>
        void ApplyWindowOpacity(double transparency);

        /// <summary>
        /// テーマがライトベースかどうかを判定する
        /// </summary>
        /// <param name="theme">判定するテーマ</param>
        /// <returns>ライトベースの場合はtrue</returns>
        bool IsLightBasedTheme(Theme theme);
    }
}
