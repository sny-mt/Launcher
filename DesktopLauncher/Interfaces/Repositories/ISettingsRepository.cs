using DesktopLauncher.Models;

namespace DesktopLauncher.Interfaces.Repositories
{
    /// <summary>
    /// 設定リポジトリのインターフェース
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        /// 設定を取得
        /// </summary>
        AppSettings Get();

        /// <summary>
        /// 設定を保存
        /// </summary>
        void Save(AppSettings settings);
    }
}
