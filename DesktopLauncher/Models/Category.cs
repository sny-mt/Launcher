using System;

namespace DesktopLauncher.Models
{
    /// <summary>
    /// カテゴリ
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 一意識別子
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 表示順序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// アイコン（絵文字またはパス）
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
