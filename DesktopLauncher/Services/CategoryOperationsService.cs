using System.Collections.ObjectModel;
using System.Linq;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// カテゴリ操作サービスの実装
    /// </summary>
    public class CategoryOperationsService : ICategoryOperationsService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IItemRepository _itemRepository;

        public CategoryOperationsService(
            ICategoryRepository categoryRepository,
            IItemRepository itemRepository)
        {
            _categoryRepository = categoryRepository;
            _itemRepository = itemRepository;
        }

        /// <inheritdoc/>
        public void MoveCategory(CategoryViewModel sourceCategory, int targetIndex, ObservableCollection<CategoryViewModel> categories)
        {
            if (sourceCategory == null || categories == null) return;

            var sourceIndex = categories.IndexOf(sourceCategory);
            if (sourceIndex < 0 || sourceIndex == targetIndex) return;
            if (targetIndex < 0 || targetIndex >= categories.Count) return;

            categories.Move(sourceIndex, targetIndex);

            // SortOrderを更新
            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Model.SortOrder = i;
                _categoryRepository.Update(categories[i].Model);
            }
            _categoryRepository.Save();
        }

        /// <inheritdoc/>
        public void MoveItemToCategory(LauncherItemViewModel item, CategoryViewModel targetCategory, ObservableCollection<CategoryViewModel> categories)
        {
            if (item == null || targetCategory == null || categories == null) return;

            // 元のカテゴリを探す
            var sourceCategory = categories.FirstOrDefault(c => c.Items.Contains(item));
            if (sourceCategory == null || sourceCategory == targetCategory) return;

            // 元のカテゴリからアイテムを削除
            sourceCategory.Items.Remove(item);

            // 新しいカテゴリにアイテムを追加
            item.Model.CategoryId = targetCategory.Id;
            item.GridPosition = -1; // 新しいカテゴリで再配置
            targetCategory.Items.Add(item);

            _itemRepository.Update(item.Model);
            _itemRepository.Save();
        }

        /// <inheritdoc/>
        public void DeleteCategory(CategoryViewModel category, ObservableCollection<CategoryViewModel> categories)
        {
            if (category == null || categories == null) return;

            // カテゴリ内のアイテムを削除
            foreach (var item in category.Items.ToList())
            {
                _itemRepository.Delete(item.Id);
            }

            _categoryRepository.Delete(category.Id);
            _categoryRepository.Save();
            _itemRepository.Save();

            categories.Remove(category);
        }
    }
}
