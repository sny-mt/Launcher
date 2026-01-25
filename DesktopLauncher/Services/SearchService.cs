using System;
using System.Collections.Generic;
using System.Linq;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// 検索サービスの実装
    /// スペース区切りでOR検索、部分一致検索
    /// </summary>
    public class SearchService : ISearchService
    {
        public IEnumerable<LauncherItem> Search(IEnumerable<LauncherItem> items, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return items;
            }

            var itemList = items.ToList();
            if (!itemList.Any())
            {
                return Enumerable.Empty<LauncherItem>();
            }

            // スペースで分割してOR検索
            var keywords = query.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(k => k.ToLower())
                                .ToArray();

            var results = itemList
                .Select(item => new
                {
                    Item = item,
                    Score = GetMatchScore(item, keywords)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Item);

            return results;
        }

        private int GetMatchScore(LauncherItem item, string[] keywords)
        {
            var nameLower = item.Name.ToLower();
            var score = 0;

            foreach (var keyword in keywords)
            {
                // 完全一致
                if (nameLower == keyword)
                {
                    score = Math.Max(score, 100);
                }
                // 先頭一致
                else if (nameLower.StartsWith(keyword))
                {
                    score = Math.Max(score, 90);
                }
                // 部分一致
                else if (nameLower.Contains(keyword))
                {
                    score = Math.Max(score, 80);
                }
            }

            return score;
        }
    }
}
