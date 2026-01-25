using System.Windows.Media;

namespace DesktopLauncher.Interfaces.Services
{
    /// <summary>
    /// アイコン取得サービスのインターフェース
    /// </summary>
    public interface IIconService
    {
        /// <summary>
        /// ファイルパスからアイコンを取得する
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>アイコン画像</returns>
        ImageSource? GetIconFromPath(string path);

        /// <summary>
        /// カスタムアイコン画像を読み込む
        /// </summary>
        /// <param name="iconPath">アイコン画像のパス</param>
        /// <returns>アイコン画像</returns>
        ImageSource? LoadCustomIcon(string iconPath);

        /// <summary>
        /// デフォルトアイコンを取得する
        /// </summary>
        /// <returns>デフォルトアイコン画像</returns>
        ImageSource GetDefaultIcon();

        /// <summary>
        /// URLからファビコンを取得してローカルに保存する
        /// </summary>
        /// <param name="url">WebサイトのURL</param>
        /// <returns>保存したアイコンのパス（失敗時はnull）</returns>
        string? DownloadFavicon(string url);
    }
}
