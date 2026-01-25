using System;
using System.Collections.Generic;
using System.Linq;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Models;

namespace DesktopLauncher.Repositories
{
    /// <summary>
    /// カテゴリリポジトリの実装
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly JsonDataStore _dataStore;

        public CategoryRepository(JsonDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public IEnumerable<Category> GetAll()
        {
            return _dataStore.Data.Categories.OrderBy(x => x.SortOrder).ToList();
        }

        public Category? GetById(string id)
        {
            return _dataStore.Data.Categories.FirstOrDefault(x => x.Id == id);
        }

        public void Add(Category entity)
        {
            var maxOrder = _dataStore.Data.Categories
                .Select(x => x.SortOrder)
                .DefaultIfEmpty(-1)
                .Max();

            entity.SortOrder = maxOrder + 1;
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;

            _dataStore.Data.Categories.Add(entity);
        }

        public void Update(Category entity)
        {
            var existing = GetById(entity.Id);
            if (existing != null)
            {
                var index = _dataStore.Data.Categories.IndexOf(existing);
                entity.UpdatedAt = DateTime.Now;
                _dataStore.Data.Categories[index] = entity;
            }
        }

        public void Delete(string id)
        {
            var category = GetById(id);
            if (category != null)
            {
                _dataStore.Data.Categories.Remove(category);
            }
        }

        public void UpdateSortOrder(IEnumerable<string> categoryIds)
        {
            var order = 0;
            foreach (var id in categoryIds)
            {
                var category = GetById(id);
                if (category != null)
                {
                    category.SortOrder = order++;
                    category.UpdatedAt = DateTime.Now;
                }
            }
        }

        public void Save()
        {
            _dataStore.Save();
        }
    }
}
