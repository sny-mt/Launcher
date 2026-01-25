using System.Collections.Generic;

namespace DesktopLauncher.Models
{
    /// <summary>
    /// ランチャーデータ（JSON保存用のルートオブジェクト）
    /// </summary>
    public class LauncherData
    {
        /// <summary>
        /// カテゴリ一覧
        /// </summary>
        public List<Category> Categories { get; set; } = new List<Category>();

        /// <summary>
        /// アイテム一覧
        /// </summary>
        public List<LauncherItem> Items { get; set; } = new List<LauncherItem>();

        /// <summary>
        /// アプリケーション設定
        /// </summary>
        public AppSettings Settings { get; set; } = new AppSettings();
    }
}
