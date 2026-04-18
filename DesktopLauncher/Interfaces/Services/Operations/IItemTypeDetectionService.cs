using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Interfaces.Services.Operations
{
    /// <summary>
    /// アイテムタイプ検出サービスのインターフェース
    /// </summary>
    public interface IItemTypeDetectionService
    {
        /// <summary>
        /// パスからアイテムタイプと名前を検出する
        /// </summary>
        /// <param name="path">ファイルパスまたはURL</param>
        /// <returns>検出されたタイプと名前のタプル</returns>
        (ItemType Type, string Name) DetectFromPath(string path);

        /// <summary>
        /// パスがURLかどうかを判定する
        /// </summary>
        /// <param name="path">検査するパス</param>
        /// <returns>URLの場合はtrue</returns>
        bool IsUrl(string path);
    }
}
