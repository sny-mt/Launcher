using System.Linq;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// アイテム操作サービスの実装
    /// </summary>
    public class ItemOperationsService : IItemOperationsService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IIconService _iconService;
        private readonly IItemTypeDetectionService _itemTypeDetectionService;
        private readonly IGridPositioningService _gridPositioningService;

        public ItemOperationsService(
            IItemRepository itemRepository,
            IIconService iconService,
            IItemTypeDetectionService itemTypeDetectionService,
            IGridPositioningService gridPositioningService)
        {
            _itemRepository = itemRepository;
            _iconService = iconService;
            _itemTypeDetectionService = itemTypeDetectionService;
            _gridPositioningService = gridPositioningService;
        }

        /// <inheritdoc/>
        public LauncherItemViewModel? AddItem(string path, CategoryViewModel category)
        {
            if (category == null || string.IsNullOrEmpty(path)) return null;

            var (itemType, name) = _itemTypeDetectionService.DetectFromPath(path);

            var item = new LauncherItem
            {
                Name = name,
                Path = path,
                ItemType = itemType,
                CategoryId = category.Id
            };

            _itemRepository.Add(item);
            _itemRepository.Save();

            var itemVm = new LauncherItemViewModel(item, _iconService);
            category.Items.Add(itemVm);

            return itemVm;
        }

        /// <inheritdoc/>
        public LauncherItemViewModel? AddItemToSlot(string path, CategoryViewModel category, int targetSlot, int totalSlots)
        {
            if (category == null || string.IsNullOrEmpty(path)) return null;
            if (targetSlot < 0 || targetSlot >= totalSlots) return null;

            // 既にそのスロットにアイテムがあれば次の空きスロットを探す
            var existingItem = category.Items.FirstOrDefault(i => i.GridPosition == targetSlot);
            if (existingItem != null)
            {
                var usedSlots = _gridPositioningService.GetUsedSlots(category.Items);
                var nextSlot = _gridPositioningService.FindNextAvailableSlot(usedSlots, 0);
                if (nextSlot >= 0)
                {
                    targetSlot = nextSlot;
                }
                else
                {
                    return null; // 空きスロットがない
                }
            }

            var (itemType, name) = _itemTypeDetectionService.DetectFromPath(path);

            var item = new LauncherItem
            {
                Name = name,
                Path = path,
                ItemType = itemType,
                CategoryId = category.Id,
                GridPosition = targetSlot
            };

            _itemRepository.Add(item);
            _itemRepository.Save();

            var itemVm = new LauncherItemViewModel(item, _iconService);
            category.Items.Add(itemVm);

            return itemVm;
        }

        /// <inheritdoc/>
        public void DeleteItem(LauncherItemViewModel item, CategoryViewModel category)
        {
            if (item == null || category == null) return;

            _itemRepository.Delete(item.Id);
            _itemRepository.Save();

            category.Items.Remove(item);
        }

        /// <inheritdoc/>
        public void MoveItemToSlot(LauncherItemViewModel item, int targetSlot, CategoryViewModel category, int totalSlots)
        {
            if (category == null || targetSlot < 0 || targetSlot >= totalSlots) return;

            // ターゲットスロットに既にアイテムがあれば入れ替え
            var existingItem = category.Items.FirstOrDefault(i => i.GridPosition == targetSlot);
            if (existingItem != null && existingItem != item)
            {
                existingItem.GridPosition = item.GridPosition;
                _itemRepository.Update(existingItem.Model);
            }

            // アイテムを新しいスロットに移動
            item.GridPosition = targetSlot;
            _itemRepository.Update(item.Model);
            _itemRepository.Save();
        }

        /// <inheritdoc/>
        public void AutoPositionNewItems(CategoryViewModel category, int totalSlots)
        {
            if (category == null) return;

            var items = category.Items.ToList();
            var unpositionedItems = items.Where(i => i.GridPosition < 0).ToList();

            if (!unpositionedItems.Any()) return;

            var usedSlots = _gridPositioningService.GetUsedSlots(items);

            foreach (var item in unpositionedItems)
            {
                var nextSlot = _gridPositioningService.FindNextAvailableSlot(usedSlots, 0);
                if (nextSlot >= 0 && nextSlot < totalSlots)
                {
                    item.GridPosition = nextSlot;
                    usedSlots.Add(nextSlot);
                    _itemRepository.Update(item.Model);
                }
            }
            _itemRepository.Save();
        }

        /// <inheritdoc/>
        public void ArrangeItems(CategoryViewModel category, int totalSlots)
        {
            if (category == null) return;

            var items = category.Items
                .OrderBy(i => i.GridPosition < 0 ? int.MaxValue : i.GridPosition)
                .ToList();

            for (int i = 0; i < items.Count && i < totalSlots; i++)
            {
                items[i].GridPosition = i;
                _itemRepository.Update(items[i].Model);
            }
            _itemRepository.Save();
        }
    }
}
