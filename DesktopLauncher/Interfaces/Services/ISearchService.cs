using System.Collections.Generic;
using DesktopLauncher.Models;

namespace DesktopLauncher.Interfaces.Services
{
    /// <summary>
    /// 検索サービスのインターフェース
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// アイテムを検索する（あいまい検索対応）
        /// </summary>
        /// <param name="items">検索対象のアイテム一覧</param>
        /// <param name="query">検索クエリ</param>
        /// <returns>マッチしたアイテム一覧（スコア順）</returns>
        IEnumerable<LauncherItem> Search(IEnumerable<LauncherItem> items, string query);
    }
}
