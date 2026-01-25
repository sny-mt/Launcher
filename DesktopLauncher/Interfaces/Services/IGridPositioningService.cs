using System.Collections.Generic;
using DesktopLauncher.Models.Enums;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Interfaces.Services
{
    /// <summary>
    /// グリッド位置管理サービスのインターフェース
    /// </summary>
    public interface IGridPositioningService
    {
        /// <summary>
        /// 現在のグリッドの列数
        /// </summary>
        int GridColumns { get; }

        /// <summary>
        /// 現在のグリッドの行数
        /// </summary>
        int GridRows { get; }

        /// <summary>
        /// 総スロット数
        /// </summary>
        int TotalSlots { get; }

        /// <summary>
        /// タイルサイズに応じてグリッドサイズを更新する
        /// </summary>
        /// <param name="tileSize">タイルサイズ</param>
        void UpdateGridSize(TileSize tileSize);

        /// <summary>
        /// 使用中スロットから次の空きスロットを探す
        /// </summary>
        /// <param name="usedSlots">使用中のスロットインデックス</param>
        /// <param name="startSlot">検索開始位置（省略時は0）</param>
        /// <returns>空きスロットのインデックス、見つからない場合は-1</returns>
        int FindNextAvailableSlot(IEnumerable<int> usedSlots, int startSlot = 0);

        /// <summary>
        /// アイテムリストから使用中のスロットを取得する
        /// </summary>
        /// <param name="items">アイテムのコレクション</param>
        /// <returns>使用中のスロットインデックスのセット</returns>
        HashSet<int> GetUsedSlots(IEnumerable<LauncherItemViewModel> items);

        /// <summary>
        /// グリッドスロットリストを作成する
        /// </summary>
        /// <returns>グリッドスロットのリスト</returns>
        List<GridSlotViewModel> CreateGridSlots();
    }
}
