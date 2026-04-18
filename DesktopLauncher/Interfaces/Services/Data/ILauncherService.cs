using DesktopLauncher.Models;

namespace DesktopLauncher.Interfaces.Services.Data
{
    /// <summary>
    /// アイテム起動サービスのインターフェース
    /// </summary>
    public interface ILauncherService
    {
        /// <summary>
        /// アイテムを起動する
        /// </summary>
        /// <param name="item">起動するアイテム</param>
        /// <param name="runAsAdmin">管理者として実行するか</param>
        /// <returns>起動成功したかどうか</returns>
        bool Launch(LauncherItem item, bool runAsAdmin = false);

        /// <summary>
        /// ファイルの場所を開く
        /// </summary>
        /// <param name="item">対象アイテム</param>
        /// <returns>成功したかどうか</returns>
        bool OpenFileLocation(LauncherItem item);
    }
}
