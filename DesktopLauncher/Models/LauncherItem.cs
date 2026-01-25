using System;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Models
{
    /// <summary>
    /// ランチャーに登録されるアイテム（アプリ/ファイル/フォルダ）
    /// </summary>
    public class LauncherItem
    {
        /// <summary>
        /// 一意識別子
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ファイル/フォルダのパス
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// アイテムの種類
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// 所属カテゴリID
        /// </summary>
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        /// カスタムアイコンのパス（nullの場合は自動取得）
        /// </summary>
        public string? CustomIconPath { get; set; }

        /// <summary>
        /// 表示順序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// グリッド上の位置（0〜39、-1は未配置）
        /// </summary>
        public int GridPosition { get; set; } = -1;

        /// <summary>
        /// 起動時の引数
        /// </summary>
        public string? Arguments { get; set; }

        /// <summary>
        /// 作業ディレクトリ
        /// </summary>
        public string? WorkingDirectory { get; set; }

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
