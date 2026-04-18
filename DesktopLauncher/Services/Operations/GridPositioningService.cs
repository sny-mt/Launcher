using System.Collections.Generic;
using System.Linq;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models.Enums;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Services.Operations
{
    /// <summary>
    /// グリッド位置管理サービスの実装
    /// </summary>
    public class GridPositioningService : IGridPositioningService
    {
        /// <inheritdoc/>
        public int GridColumns { get; private set; } = 8;

        /// <inheritdoc/>
        public int GridRows { get; private set; } = 5;

        /// <inheritdoc/>
        public int TotalSlots => GridColumns * GridRows;

        /// <inheritdoc/>
        public void UpdateGridSize(TileSize tileSize)
        {
            switch (tileSize)
            {
                case TileSize.Small:
                    GridColumns = 10;
                    GridRows = 6;
                    break;
                case TileSize.Large:
                    GridColumns = 6;
                    GridRows = 4;
                    break;
                default: // Medium
                    GridColumns = 8;
                    GridRows = 5;
                    break;
            }
        }

        /// <inheritdoc/>
        public int FindNextAvailableSlot(IEnumerable<int> usedSlots, int startSlot = 0)
        {
            var usedSet = usedSlots as HashSet<int> ?? usedSlots.ToHashSet();

            for (int slot = startSlot; slot < TotalSlots; slot++)
            {
                if (!usedSet.Contains(slot))
                {
                    return slot;
                }
            }

            return -1;
        }

        /// <inheritdoc/>
        public HashSet<int> GetUsedSlots(IEnumerable<LauncherItemViewModel> items)
        {
            return items
                .Where(i => i.GridPosition >= 0)
                .Select(i => i.GridPosition)
                .ToHashSet();
        }

        /// <inheritdoc/>
        public List<GridSlotViewModel> CreateGridSlots()
        {
            var slots = new List<GridSlotViewModel>();
            for (int i = 0; i < TotalSlots; i++)
            {
                slots.Add(new GridSlotViewModel { SlotIndex = i });
            }
            return slots;
        }
    }
}
