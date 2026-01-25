using System.Collections.Generic;
using DesktopLauncher.Models;

namespace DesktopLauncher.Interfaces.Repositories
{
    /// <summary>
    /// ランチャーアイテムリポジトリのインターフェース
    /// </summary>
    public interface IItemRepository : IRepository<LauncherItem>
    {
        /// <summary>
        /// カテゴリIDでアイテムを取得
        /// </summary>
        /// <param name="categoryId">カテゴリID</param>
        /// <returns>該当するアイテム一覧</returns>
        IEnumerable<LauncherItem> GetByCategory(string categoryId);

        /// <summary>
        /// 並び順を更新
        /// </summary>
        /// <param name="itemIds">アイテムIDの順序</param>
        void UpdateSortOrder(IEnumerable<string> itemIds);
    }
}
