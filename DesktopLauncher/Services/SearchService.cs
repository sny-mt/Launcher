using System;
using System.Collections.Generic;
using System.Linq;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// 検索サービスの実装。
    /// クエリを空白（半角/全角/タブ）で分割し、各キーワードを Name と Path に対して評価する。
    /// </summary>
    /// <remarks>
    /// スコア表:
    /// <list type="bullet">
    ///   <item><description>Name 完全一致: 100</description></item>
    ///   <item><description>Name 先頭一致: 90</description></item>
    ///   <item><description>Name 部分一致: 80</description></item>
    ///   <item><description>Path マッチ: 上記の半分（整数切り捨て）</description></item>
    ///   <item><description>キーワードごとに加算（マッチしたキーワード数が多いほど高スコア）</description></item>
    ///   <item><description>お気に入り: +10</description></item>
    ///   <item><description>起動回数: +min(LaunchCount, 5)</description></item>
    /// </list>
    /// </remarks>
    public class SearchService : ISearchService
    {
        private const int ExactMatchScore = 100;
        private const int PrefixMatchScore = 90;
        private const int ContainsMatchScore = 80;
        private const int FavoriteBonus = 10;
        private const int MaxLaunchCountBonus = 5;

        private static readonly char[] KeywordSeparators = { ' ', '　', '\t' };

        public IEnumerable<LauncherItem> Search(IEnumerable<LauncherItem> items, string query)
        {
            ArgumentNullException.ThrowIfNull(items);

            if (string.IsNullOrWhiteSpace(query))
            {
                return items;
            }

            var keywords = query.Split(KeywordSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (keywords.Length == 0)
            {
                return items;
            }

            return items
                .Select(item => (Item: item, Score: GetMatchScore(item, keywords)))
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.LastLaunchedAt ?? DateTime.MinValue)
                .ThenBy(x => x.Item.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.Item)
                .ToList();
        }

        private static int GetMatchScore(LauncherItem item, string[] keywords)
        {
            var score = 0;
            foreach (var keyword in keywords)
            {
                var nameScore = GetFieldScore(item.Name, keyword);
                var pathScore = GetFieldScore(item.Path, keyword) / 2;
                score += Math.Max(nameScore, pathScore);
            }

            if (score > 0)
            {
                if (item.IsFavorite)
                {
                    score += FavoriteBonus;
                }

                score += Math.Clamp(item.LaunchCount, 0, MaxLaunchCountBonus);
            }

            return score;
        }

        private static int GetFieldScore(string? field, string keyword)
        {
            if (string.IsNullOrEmpty(field))
            {
                return 0;
            }

            if (field.Equals(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return ExactMatchScore;
            }

            if (field.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return PrefixMatchScore;
            }

            if (field.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return ContainsMatchScore;
            }

            return 0;
        }
    }
}
