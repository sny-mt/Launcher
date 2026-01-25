using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Interfaces.Services
{
    /// <summary>
    /// アイテム操作サービスのインターフェース
    /// </summary>
    public interface IItemOperationsService
    {
        /// <summary>
        /// パスからアイテムを追加する
        /// </summary>
        /// <param name="path">ファイルパスまたはURL</param>
        /// <param name="category">追加先のカテゴリ</param>
        /// <returns>作成されたアイテムのViewModel</returns>
        LauncherItemViewModel? AddItem(string path, CategoryViewModel category);

        /// <summary>
        /// 指定スロットにアイテムを追加する
        /// </summary>
        /// <param name="path">ファイルパスまたはURL</param>
        /// <param name="category">追加先のカテゴリ</param>
        /// <param name="targetSlot">ターゲットスロット</param>
        /// <param name="totalSlots">総スロット数</param>
        /// <returns>作成されたアイテムのViewModel</returns>
        LauncherItemViewModel? AddItemToSlot(string path, CategoryViewModel category, int targetSlot, int totalSlots);

        /// <summary>
        /// アイテムを削除する
        /// </summary>
        /// <param name="item">削除するアイテム</param>
        /// <param name="category">アイテムが属するカテゴリ</param>
        void DeleteItem(LauncherItemViewModel item, CategoryViewModel category);

        /// <summary>
        /// アイテムを新しいスロットに移動する
        /// </summary>
        /// <param name="item">移動するアイテム</param>
        /// <param name="targetSlot">ターゲットスロット</param>
        /// <param name="category">アイテムが属するカテゴリ</param>
        /// <param name="totalSlots">総スロット数</param>
        void MoveItemToSlot(LauncherItemViewModel item, int targetSlot, CategoryViewModel category, int totalSlots);

        /// <summary>
        /// 配置されていないアイテムを自動配置する
        /// </summary>
        /// <param name="category">対象のカテゴリ</param>
        /// <param name="totalSlots">総スロット数</param>
        void AutoPositionNewItems(CategoryViewModel category, int totalSlots);

        /// <summary>
        /// アイテムを順番に整列する
        /// </summary>
        /// <param name="category">対象のカテゴリ</param>
        /// <param name="totalSlots">総スロット数</param>
        void ArrangeItems(CategoryViewModel category, int totalSlots);
    }
}
