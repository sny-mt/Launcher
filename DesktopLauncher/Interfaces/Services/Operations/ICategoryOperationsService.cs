using System.Collections.ObjectModel;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Interfaces.Services.Operations
{
    /// <summary>
    /// カテゴリ操作サービスのインターフェース
    /// </summary>
    public interface ICategoryOperationsService
    {
        /// <summary>
        /// カテゴリを並べ替える
        /// </summary>
        /// <param name="sourceCategory">移動するカテゴリ</param>
        /// <param name="targetIndex">移動先のインデックス</param>
        /// <param name="categories">カテゴリコレクション</param>
        void MoveCategory(CategoryViewModel sourceCategory, int targetIndex, ObservableCollection<CategoryViewModel> categories);

        /// <summary>
        /// アイテムを別のカテゴリに移動する
        /// </summary>
        /// <param name="item">移動するアイテム</param>
        /// <param name="targetCategory">移動先のカテゴリ</param>
        /// <param name="categories">カテゴリコレクション</param>
        void MoveItemToCategory(LauncherItemViewModel item, CategoryViewModel targetCategory, ObservableCollection<CategoryViewModel> categories);

        /// <summary>
        /// カテゴリを削除する
        /// </summary>
        /// <param name="category">削除するカテゴリ</param>
        /// <param name="categories">カテゴリコレクション</param>
        void DeleteCategory(CategoryViewModel category, ObservableCollection<CategoryViewModel> categories);
    }
}
