using System.Windows.Media;

namespace DesktopLauncher.Interfaces.Services.Icons
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

        /// <summary>
        /// ファイルパスからアイコンを取得しBase64文字列として返す
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>Base64エンコードされたPNG画像（失敗時はnull）</returns>
        string? GetIconBase64FromPath(string path);

        /// <summary>
        /// URLからファビコンを取得しBase64文字列として返す
        /// </summary>
        /// <param name="url">WebサイトのURL</param>
        /// <returns>Base64エンコードされたPNG画像（失敗時はnull）</returns>
        string? DownloadFaviconAsBase64(string url);

        /// <summary>
        /// Base64文字列からアイコンを読み込む
        /// </summary>
        /// <param name="base64">Base64エンコードされた画像</param>
        /// <returns>アイコン画像</returns>
        ImageSource? LoadFromBase64(string base64);

        /// <summary>
        /// ImageSourceをBase64文字列に変換する
        /// </summary>
        /// <param name="imageSource">変換する画像</param>
        /// <param name="size">出力サイズ（ピクセル）</param>
        /// <returns>Base64エンコードされたPNG画像</returns>
        string? ConvertToBase64(ImageSource imageSource, int size = 48);
    }
}
