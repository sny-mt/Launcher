using System;
using System.Collections.Generic;
using System.Linq;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Models;

namespace DesktopLauncher.Repositories
{
    /// <summary>
    /// ランチャーアイテムリポジトリの実装
    /// </summary>
    public class ItemRepository : IItemRepository
    {
        private readonly JsonDataStore _dataStore;

        public ItemRepository(JsonDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public IEnumerable<LauncherItem> GetAll()
        {
            return _dataStore.Data.Items.OrderBy(x => x.SortOrder).ToList();
        }

        public LauncherItem? GetById(string id)
        {
            return _dataStore.Data.Items.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<LauncherItem> GetByCategory(string categoryId)
        {
            return _dataStore.Data.Items
                .Where(x => x.CategoryId == categoryId)
                .OrderBy(x => x.SortOrder)
                .ToList();
        }

        public void Add(LauncherItem entity)
        {
            // 新しいアイテムは末尾に追加
            var maxOrder = _dataStore.Data.Items
                .Where(x => x.CategoryId == entity.CategoryId)
                .Select(x => x.SortOrder)
                .DefaultIfEmpty(-1)
                .Max();

            entity.SortOrder = maxOrder + 1;
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;

            _dataStore.Data.Items.Add(entity);
        }

        public void Update(LauncherItem entity)
        {
            var existing = GetById(entity.Id);
            if (existing != null)
            {
                var index = _dataStore.Data.Items.IndexOf(existing);
                entity.UpdatedAt = DateTime.Now;
                _dataStore.Data.Items[index] = entity;
            }
        }

        public void Delete(string id)
        {
            var item = GetById(id);
            if (item != null)
            {
                _dataStore.Data.Items.Remove(item);
            }
        }

        public void UpdateSortOrder(IEnumerable<string> itemIds)
        {
            var order = 0;
            foreach (var id in itemIds)
            {
                var item = GetById(id);
                if (item != null)
                {
                    item.SortOrder = order++;
                    item.UpdatedAt = DateTime.Now;
                }
            }
        }

        public void Save()
        {
            _dataStore.Save();
        }
    }
}
