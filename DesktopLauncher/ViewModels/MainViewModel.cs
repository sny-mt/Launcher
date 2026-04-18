using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models;
using DesktopLauncher.Models.Enums;
using DesktopLauncher.Services;
using DesktopLauncher.ViewModels.Base;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// メインウィンドウのViewModel
    /// </summary>
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IItemRepository _itemRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IIconService _iconService;
        private readonly ILauncherService _launcherService;
        private readonly ISearchService _searchService;
        private readonly IDialogService _dialogService;
        private readonly IGridPositioningService _gridPositioningService;
        private readonly IItemOperationsService _itemOperationsService;
        private readonly ICategoryOperationsService _categoryOperationsService;

        [ObservableProperty]
        private ObservableCollection<CategoryViewModel> _categories;

        [ObservableProperty]
        private CategoryViewModel? _selectedCategory;

        [ObservableProperty]
        private ObservableCollection<LauncherItemViewModel> _displayedItems;

        [ObservableProperty]
        private ObservableCollection<GridSlotViewModel> _gridSlots;

        [ObservableProperty]
        private ObservableCollection<GridSlotViewModel> _searchGridSlots;

        [ObservableProperty]
        private ObservableCollection<CategoryViewModel> _filteredCategories;

        [ObservableProperty]
        private LauncherItemViewModel? _selectedItem;

        [ObservableProperty]
        private int _gridColumns = 8;

        [ObservableProperty]
        private int _gridRows = 5;

        private int TotalSlots => GridColumns * GridRows;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private AppSettings _settings;

        [ObservableProperty]
        private bool _isWindowVisible = true;

        [ObservableProperty]
        private ObservableCollection<ToastViewModel> _toasts = new();

        [ObservableProperty]
        private int _selectedSlotIndex = -1;

        public bool ShowTileView => string.IsNullOrWhiteSpace(SearchText);
        public bool HasSearchText => !string.IsNullOrWhiteSpace(SearchText);

        partial void OnSelectedSlotIndexChanged(int value)
        {
            // グリッドスロットの選択状態を更新
            for (int i = 0; i < GridSlots.Count; i++)
            {
                GridSlots[i].IsSelected = (i == value);
            }
            // 検索グリッドスロットの選択状態を更新
            for (int i = 0; i < SearchGridSlots.Count; i++)
            {
                SearchGridSlots[i].IsSelected = (i == value);
            }
        }

        public event EventHandler? RequestHideWindow;
        public event EventHandler? RequestDeactivate;

        public MainViewModel(
            IItemRepository itemRepository,
            ICategoryRepository categoryRepository,
            ISettingsRepository settingsRepository,
            IIconService iconService,
            ILauncherService launcherService,
            ISearchService searchService,
            IDialogService dialogService,
            IGridPositioningService gridPositioningService,
            IItemOperationsService itemOperationsService,
            ICategoryOperationsService categoryOperationsService)
        {
            _itemRepository = itemRepository;
            _categoryRepository = categoryRepository;
            _settingsRepository = settingsRepository;
            _iconService = iconService;
            _launcherService = launcherService;
            _searchService = searchService;
            _dialogService = dialogService;
            _gridPositioningService = gridPositioningService;
            _itemOperationsService = itemOperationsService;
            _categoryOperationsService = categoryOperationsService;

            _categories = new ObservableCollection<CategoryViewModel>();
            _displayedItems = new ObservableCollection<LauncherItemViewModel>();
            _gridSlots = new ObservableCollection<GridSlotViewModel>();
            _searchGridSlots = new ObservableCollection<GridSlotViewModel>();
            _filteredCategories = new ObservableCollection<CategoryViewModel>();
            _settings = _settingsRepository.Get();

            InitializeGridSlots();
            LoadData();
        }

        private void InitializeGridSlots()
        {
            UpdateGridSize();
            RefreshGridSlotCollection();
        }

        private void UpdateGridSize()
        {
            _gridPositioningService.UpdateGridSize(Settings.TileSize);
            GridColumns = _gridPositioningService.GridColumns;
            GridRows = _gridPositioningService.GridRows;
        }

        private void RefreshGridSlotCollection()
        {
            GridSlots.Clear();
            foreach (var slot in _gridPositioningService.CreateGridSlots())
            {
                GridSlots.Add(slot);
            }
        }

        public void RefreshGridSlots()
        {
            var oldTotalSlots = GridSlots.Count;
            UpdateGridSize();

            if (oldTotalSlots != TotalSlots)
            {
                RefreshGridSlotCollection();
                RelocateOutOfRangeItems();
                FilterItems();
            }
        }

        private void RelocateOutOfRangeItems()
        {
            if (SelectedCategory == null) return;

            var itemsOutOfRange = SelectedCategory.Items.Where(i => i.GridPosition >= TotalSlots).ToList();
            var usedSlots = _gridPositioningService.GetUsedSlots(
                SelectedCategory.Items.Where(i => i.GridPosition >= 0 && i.GridPosition < TotalSlots));

            foreach (var item in itemsOutOfRange)
            {
                var newSlot = _gridPositioningService.FindNextAvailableSlot(usedSlots, 0);
                if (newSlot >= 0 && newSlot < TotalSlots)
                {
                    item.GridPosition = newSlot;
                    usedSlots.Add(newSlot);
                    _itemRepository.Update(item.Model);
                }
            }
            _itemRepository.Save();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterItems();
            OnPropertyChanged(nameof(ShowTileView));
            OnPropertyChanged(nameof(HasSearchText));

            // 検索テキストが変わったら選択をリセット
            SelectedSlotIndex = -1;
        }

        partial void OnSelectedCategoryChanged(CategoryViewModel? value)
        {
            if (value != null)
            {
                _itemOperationsService.AutoPositionNewItems(value, TotalSlots);
            }
            FilterItems();
        }

        private void LoadData()
        {
            Categories.Clear();

            // 既存アイテムの移行処理
            MigrateIconsToBase64();

            foreach (var category in _categoryRepository.GetAll())
            {
                var categoryVm = new CategoryViewModel(category);

                var items = _itemRepository.GetByCategory(category.Id);
                foreach (var item in items)
                {
                    categoryVm.Items.Add(new LauncherItemViewModel(item, _iconService));
                }

                Categories.Add(categoryVm);
            }

            if (Categories.Any())
            {
                SelectedCategory = Categories.First();
            }
        }

        /// <summary>
        /// 既存アイテムのアイコンをBase64形式に移行する
        /// </summary>
        private void MigrateIconsToBase64()
        {
            var allItems = _itemRepository.GetAll();
            var needsSave = false;

            foreach (var item in allItems)
            {
                // 既にBase64アイコンがある場合はスキップ
                if (!string.IsNullOrEmpty(item.IconBase64))
                {
                    continue;
                }

                // CustomIconPathからBase64に変換
                if (!string.IsNullOrEmpty(item.CustomIconPath))
                {
                    var customIcon = _iconService.LoadCustomIcon(item.CustomIconPath!);
                    if (customIcon != null)
                    {
                        item.IconBase64 = _iconService.ConvertToBase64(customIcon);
                        item.CustomIconPath = null; // パスをクリア
                        _itemRepository.Update(item);
                        needsSave = true;
                        continue;
                    }
                }

                // パスからアイコンを取得してBase64に変換
                if (!string.IsNullOrEmpty(item.Path))
                {
                    item.IconBase64 = _iconService.GetIconBase64FromPath(item.Path);
                    if (!string.IsNullOrEmpty(item.IconBase64))
                    {
                        _itemRepository.Update(item);
                        needsSave = true;
                    }
                }
            }

            if (needsSave)
            {
                _itemRepository.Save();
            }
        }

        private void FilterItems()
        {
            DisplayedItems.Clear();

            foreach (var slot in GridSlots)
            {
                slot.Item = null;
            }

            var hasSearch = !string.IsNullOrWhiteSpace(SearchText);

            // すべてのカテゴリの検索結果数を更新
            System.Collections.Generic.IEnumerable<LauncherItem>? selectedCategorySearchResult = null;
            foreach (var category in Categories)
            {
                category.HasSearchText = hasSearch;
                if (hasSearch)
                {
                    var categoryItems = category.Items.Select(vm => vm.Model);
                    var searchResult = _searchService.Search(categoryItems, SearchText).ToList();
                    category.SearchMatchCount = searchResult.Count;

                    if (category == SelectedCategory)
                    {
                        selectedCategorySearchResult = searchResult;
                    }
                }
                else
                {
                    category.SearchMatchCount = 0;
                }
            }

            // フィルタ済みカテゴリを更新（マッチ数が多い順）
            FilteredCategories.Clear();
            if (hasSearch)
            {
                foreach (var category in Categories
                    .Where(c => c.SearchMatchCount > 0)
                    .OrderByDescending(c => c.SearchMatchCount))
                {
                    FilteredCategories.Add(category);
                }
            }

            if (SelectedCategory == null) return;

            if (hasSearch)
            {
                var items = selectedCategorySearchResult
                    ?? _searchService.Search(SelectedCategory.Items.Select(vm => vm.Model), SearchText);
                foreach (var item in items)
                {
                    var vm = SelectedCategory.Items.FirstOrDefault(x => x.Id == item.Id);
                    if (vm != null)
                    {
                        DisplayedItems.Add(vm);
                    }
                }

                // SearchGridSlotsを更新
                SearchGridSlots.Clear();
                for (int i = 0; i < TotalSlots; i++)
                {
                    var slot = new GridSlotViewModel { SlotIndex = i };
                    if (i < DisplayedItems.Count)
                    {
                        slot.Item = DisplayedItems[i];
                    }
                    SearchGridSlots.Add(slot);
                }
            }
            else
            {
                foreach (var itemVm in SelectedCategory.Items)
                {
                    var pos = itemVm.GridPosition;
                    if (pos >= 0 && pos < TotalSlots)
                    {
                        GridSlots[pos].Item = itemVm;
                    }
                }
            }

            // 選択状態を再適用
            RefreshSelectionState();
        }

        /// <summary>
        /// スロットの選択状態を再適用する
        /// </summary>
        private void RefreshSelectionState()
        {
            var currentSelection = SelectedSlotIndex;
            for (int i = 0; i < GridSlots.Count; i++)
            {
                GridSlots[i].IsSelected = (i == currentSelection);
            }
            for (int i = 0; i < SearchGridSlots.Count; i++)
            {
                SearchGridSlots[i].IsSelected = (i == currentSelection);
            }
        }

        [RelayCommand]
        private void LaunchItem(LauncherItemViewModel? item)
        {
            if (item == null) return;

            var success = _launcherService.Launch(item.Model);
            if (success)
            {
                _itemRepository.Update(item.Model);
                _itemRepository.Save();
                RequestDeactivate?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _dialogService.ShowError($"'{item.Name}' の起動に失敗しました。");
            }
        }

        [RelayCommand]
        private void LaunchItemAsAdmin(LauncherItemViewModel? item)
        {
            if (item == null) return;

            var success = _launcherService.Launch(item.Model, runAsAdmin: true);
            if (success)
            {
                _itemRepository.Update(item.Model);
                _itemRepository.Save();
                RequestDeactivate?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _dialogService.ShowError($"'{item.Name}' の起動に失敗しました。");
            }
        }

        [RelayCommand]
        private void ToggleFavorite(LauncherItemViewModel? item)
        {
            if (item == null) return;

            item.Model.IsFavorite = !item.Model.IsFavorite;
            _itemRepository.Update(item.Model);
            _itemRepository.Save();

            OnPropertyChanged(nameof(DisplayedItems));
            ShowToast(item.Model.IsFavorite ? $"'{item.Name}' をお気に入りに追加しました" : $"'{item.Name}' をお気に入りから外しました");
        }

        [RelayCommand]
        private void LaunchAllInCategory()
        {
            if (SelectedCategory == null || !SelectedCategory.Items.Any()) return;

            if (!_dialogService.ShowConfirmDialog($"カテゴリ '{SelectedCategory.Name}' のアイテムをすべて起動しますか？"))
            {
                return;
            }

            var count = 0;
            foreach (var itemVm in SelectedCategory.Items)
            {
                if (_launcherService.Launch(itemVm.Model))
                {
                    _itemRepository.Update(itemVm.Model);
                    count++;
                }
            }

            if (count > 0)
            {
                _itemRepository.Save();
                ShowToast($"{count} 個のアイテムを起動しました");
            }
        }

        [RelayCommand]
        private void OpenFileLocation(LauncherItemViewModel? item)
        {
            if (item == null) return;

            if (!_launcherService.OpenFileLocation(item.Model))
            {
                _dialogService.ShowError("ファイルの場所を開けませんでした。");
            }
        }

        [RelayCommand]
        private void AddItem()
        {
            if (SelectedCategory == null)
            {
                _dialogService.ShowError("カテゴリを選択してください。");
                return;
            }

            var path = _dialogService.ShowFileOrFolderDialog();
            if (string.IsNullOrEmpty(path)) return;

            AddItemFromPath(path!);
        }

        [RelayCommand]
        private void AddUrl()
        {
            if (SelectedCategory == null)
            {
                _dialogService.ShowError("カテゴリを選択してください。");
                return;
            }

            var input = _dialogService.ShowInputDialog("URLを入力してください", "URL追加", "https://");
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!UrlHelper.TryNormalizeHttpUrl(input, out var normalizedUrl))
            {
                _dialogService.ShowError("有効なURLを入力してください。");
                return;
            }

            AddItemFromPath(normalizedUrl);
        }

        public void AddItemFromPath(string path)
        {
            if (SelectedCategory == null) return;

            var itemVm = _itemOperationsService.AddItem(path, SelectedCategory);
            if (itemVm != null)
            {
                _itemOperationsService.AutoPositionNewItems(SelectedCategory, TotalSlots);
                FilterItems();
                ShowToast($"'{itemVm.Name}' を追加しました");
            }
        }

        [RelayCommand]
        private void DeleteItem(LauncherItemViewModel? item)
        {
            if (item == null || SelectedCategory == null) return;

            var itemName = item.Name;
            if (!_dialogService.ShowConfirmDialog($"'{itemName}' を削除しますか？"))
            {
                return;
            }

            _itemOperationsService.DeleteItem(item, SelectedCategory);
            FilterItems();
            ShowToast($"'{itemName}' を削除しました");
        }

        public void MoveItemToSlot(LauncherItemViewModel item, int targetSlot)
        {
            if (SelectedCategory == null) return;

            _itemOperationsService.MoveItemToSlot(item, targetSlot, SelectedCategory, TotalSlots);
            FilterItems();
        }

        public void AddItemToSlot(string path, int targetSlot)
        {
            if (SelectedCategory == null) return;

            var itemVm = _itemOperationsService.AddItemToSlot(path, SelectedCategory, targetSlot, TotalSlots);
            if (itemVm != null)
            {
                FilterItems();
            }
        }

        public void AddItemsToSlot(string[] paths, int startSlot)
        {
            if (SelectedCategory == null || paths == null || paths.Length == 0) return;

            var usedSlots = _gridPositioningService.GetUsedSlots(SelectedCategory.Items);
            var currentSlot = startSlot;

            foreach (var path in paths)
            {
                while (usedSlots.Contains(currentSlot) && currentSlot < TotalSlots)
                {
                    currentSlot++;
                }

                if (currentSlot >= TotalSlots) break;

                var itemVm = _itemOperationsService.AddItemToSlot(path, SelectedCategory, currentSlot, TotalSlots);
                if (itemVm != null)
                {
                    usedSlots.Add(currentSlot);
                }
                currentSlot++;
            }

            FilterItems();
        }

        [RelayCommand]
        private void ArrangeItems()
        {
            if (SelectedCategory == null) return;

            _itemOperationsService.ArrangeItems(SelectedCategory, TotalSlots);
            FilterItems();
        }

        [RelayCommand]
        private void SelectCategory(CategoryViewModel? category)
        {
            if (category != null)
            {
                SelectedCategory = category;
            }
        }

        [RelayCommand]
        private void AddCategory()
        {
            var editWindow = new Views.CategoryEditWindow();
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.ResultCategory != null)
            {
                var category = editWindow.ResultCategory;
                _categoryRepository.Add(category);
                _categoryRepository.Save();

                var categoryVm = new CategoryViewModel(category);
                Categories.Add(categoryVm);
                SelectedCategory = categoryVm;
                ShowToast($"カテゴリ '{category.Name}' を追加しました");
            }
        }

        [RelayCommand]
        private void EditCategory(CategoryViewModel? categoryVm)
        {
            if (categoryVm == null) return;

            var editWindow = new Views.CategoryEditWindow(categoryVm.Model);
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.ResultCategory != null)
            {
                var updatedCategory = editWindow.ResultCategory;
                _categoryRepository.Update(updatedCategory);
                _categoryRepository.Save();

                categoryVm.UpdateModel(updatedCategory);
                ShowToast($"カテゴリ '{updatedCategory.Name}' を更新しました");
            }
        }

        [RelayCommand]
        private void DeleteCategory(CategoryViewModel? category)
        {
            if (category == null) return;

            var categoryName = category.Name;
            if (!_dialogService.ShowConfirmDialog($"カテゴリ '{categoryName}' を削除しますか？\n含まれるアイテムも削除されます。"))
            {
                return;
            }

            var wasSelected = SelectedCategory == category;
            _categoryOperationsService.DeleteCategory(category, Categories);

            if (wasSelected)
            {
                SelectedCategory = Categories.FirstOrDefault();
            }
            ShowToast($"カテゴリ '{categoryName}' を削除しました");
        }

        [RelayCommand]
        private void ShowSettings()
        {
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.Owner = Application.Current.MainWindow;

            if (settingsWindow.ShowDialog() == true && settingsWindow.ResultSettings != null)
            {
                Settings = settingsWindow.ResultSettings;
                RefreshGridSlots();
                SettingsChanged?.Invoke(this, Settings);
                ShowToast("設定を保存しました");
            }
        }

        [RelayCommand]
        private void EditItem(LauncherItemViewModel? item)
        {
            if (item == null || SelectedCategory == null) return;

            var editWindow = new Views.ItemEditWindow(item.Model);
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.ResultItem != null)
            {
                var resultItem = editWindow.ResultItem;
                resultItem.CategoryId = SelectedCategory.Id;

                _itemRepository.Update(resultItem);
                _itemRepository.Save();

                item.UpdateModel(resultItem);
                FilterItems();
                ShowToast($"'{item.Name}' を更新しました");
            }
        }

        [RelayCommand]
        private void ToggleWindow()
        {
            IsWindowVisible = !IsWindowVisible;
        }

        public void ShowWindow()
        {
            IsWindowVisible = true;
        }

        public void HideWindow()
        {
            IsWindowVisible = false;
            SearchText = string.Empty;
            RequestHideWindow?.Invoke(this, EventArgs.Empty);
        }

        public void HandleFileDrop(string[] files)
        {
            foreach (var file in files)
            {
                AddItemFromPath(file);
            }
        }

        /// <summary>
        /// カテゴリの順序を変更する
        /// </summary>
        public void MoveCategory(CategoryViewModel sourceCategory, int targetIndex)
        {
            _categoryOperationsService.MoveCategory(sourceCategory, targetIndex, Categories);
        }

        /// <summary>
        /// アイテムを別のカテゴリに移動する
        /// </summary>
        public void MoveItemToCategory(LauncherItemViewModel item, CategoryViewModel targetCategory)
        {
            var sourceCategory = Categories.FirstOrDefault(c => c.Items.Contains(item));

            _categoryOperationsService.MoveItemToCategory(item, targetCategory, Categories);

            // 移動後に自動配置と再フィルタ
            if (SelectedCategory == sourceCategory || SelectedCategory == targetCategory)
            {
                if (SelectedCategory != null)
                {
                    _itemOperationsService.AutoPositionNewItems(SelectedCategory, TotalSlots);
                }
                FilterItems();
            }
        }

        /// <summary>
        /// 設定が変更されたときに発火するイベント
        /// </summary>
        public event EventHandler<AppSettings>? SettingsChanged;

        /// <summary>
        /// トースト通知を表示する
        /// </summary>
        public void ShowToast(string message, ToastType type = ToastType.Success)
        {
            var toast = new ToastViewModel(message, type);
            toast.CloseRequested += (s, e) =>
            {
                if (s is ToastViewModel t)
                {
                    Toasts.Remove(t);
                }
            };
            Toasts.Add(toast);
        }

        /// <summary>
        /// 次のカテゴリに切り替える
        /// </summary>
        public void SelectNextCategory()
        {
            // 検索中はフィルタ済みカテゴリを使用
            var targetCategories = HasSearchText && FilteredCategories.Count > 0
                ? FilteredCategories
                : Categories;

            if (targetCategories.Count == 0) return;

            var currentIndex = SelectedCategory != null ? targetCategories.IndexOf(SelectedCategory) : -1;
            var nextIndex = (currentIndex + 1) % targetCategories.Count;
            SelectedCategory = targetCategories[nextIndex];
            SelectedSlotIndex = -1;
        }

        /// <summary>
        /// 前のカテゴリに切り替える
        /// </summary>
        public void SelectPreviousCategory()
        {
            // 検索中はフィルタ済みカテゴリを使用
            var targetCategories = HasSearchText && FilteredCategories.Count > 0
                ? FilteredCategories
                : Categories;

            if (targetCategories.Count == 0) return;

            var currentIndex = SelectedCategory != null ? targetCategories.IndexOf(SelectedCategory) : 0;
            var prevIndex = (currentIndex - 1 + targetCategories.Count) % targetCategories.Count;
            SelectedCategory = targetCategories[prevIndex];
            SelectedSlotIndex = -1;
        }

        /// <summary>
        /// 矢印キーでスロットを移動する
        /// </summary>
        public void NavigateSlot(int deltaX, int deltaY)
        {
            if (SelectedCategory == null) return;

            // 検索中は検索結果内を移動
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                NavigateSearchResults(deltaX, deltaY);
                return;
            }

            // タイル表示モードの場合
            // 初期選択
            if (SelectedSlotIndex < 0)
            {
                // 最初のアイテムがあるスロットを選択
                var firstItem = SelectedCategory.Items.OrderBy(i => i.GridPosition).FirstOrDefault(i => i.GridPosition >= 0);
                SelectedSlotIndex = firstItem?.GridPosition ?? 0;
                return;
            }

            // 現在の行と列を計算
            var currentRow = SelectedSlotIndex / GridColumns;
            var currentCol = SelectedSlotIndex % GridColumns;

            // 新しい位置を計算
            var newCol = Math.Max(0, Math.Min(GridColumns - 1, currentCol + deltaX));
            var newRow = Math.Max(0, Math.Min(GridRows - 1, currentRow + deltaY));
            var newIndex = newRow * GridColumns + newCol;

            if (newIndex >= 0 && newIndex < TotalSlots)
            {
                SelectedSlotIndex = newIndex;
            }
        }

        private void NavigateSearchResults(int deltaX, int deltaY)
        {
            if (SearchGridSlots.Count == 0) return;

            // 初期選択: 最初のアイテムがあるスロットを選択
            if (SelectedSlotIndex < 0)
            {
                // 検索結果の最初のアイテムを選択
                for (int i = 0; i < SearchGridSlots.Count; i++)
                {
                    if (SearchGridSlots[i].HasItem)
                    {
                        SelectedSlotIndex = i;
                        return;
                    }
                }
                SelectedSlotIndex = 0;
                return;
            }

            // 現在の行と列を計算
            var currentRow = SelectedSlotIndex / GridColumns;
            var currentCol = SelectedSlotIndex % GridColumns;

            // 新しい位置を計算
            var newCol = Math.Max(0, Math.Min(GridColumns - 1, currentCol + deltaX));
            var newRow = Math.Max(0, Math.Min(GridRows - 1, currentRow + deltaY));
            var newIndex = newRow * GridColumns + newCol;

            if (newIndex >= 0 && newIndex < TotalSlots)
            {
                SelectedSlotIndex = newIndex;
            }
        }

        /// <summary>
        /// 選択中のアイテムを取得する
        /// </summary>
        public LauncherItemViewModel? GetSelectedItem()
        {
            if (SelectedSlotIndex < 0) return null;

            // 検索中は検索グリッドスロットから取得
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                if (SelectedSlotIndex < SearchGridSlots.Count)
                {
                    return SearchGridSlots[SelectedSlotIndex].Item;
                }
                return null;
            }

            // タイル表示はグリッドスロットから取得
            if (SelectedSlotIndex < GridSlots.Count)
            {
                return GridSlots[SelectedSlotIndex].Item;
            }
            return null;
        }

        /// <summary>
        /// 選択中のアイテムを起動する
        /// </summary>
        public void LaunchSelectedItem()
        {
            var item = GetSelectedItem();
            if (item != null)
            {
                LaunchItemCommand.Execute(item);
            }
        }

        /// <summary>
        /// 選択をリセットする
        /// </summary>
        public void ResetSelection()
        {
            SelectedSlotIndex = -1;
        }
    }
}
