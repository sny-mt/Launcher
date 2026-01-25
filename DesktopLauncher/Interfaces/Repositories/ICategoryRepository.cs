using System.Collections.Generic;
using DesktopLauncher.Models;

namespace DesktopLauncher.Interfaces.Repositories
{
    /// <summary>
    /// カテゴリリポジトリのインターフェース
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// 並び順を更新
        /// </summary>
        /// <param name="categoryIds">カテゴリIDの順序</param>
        void UpdateSortOrder(IEnumerable<string> categoryIds);
    }
}
