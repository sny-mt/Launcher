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

        private void FilterItems()
        {
            DisplayedItems.Clear();

            foreach (var slot in GridSlots)
            {
                slot.Item = null;
            }

            var hasSearch = !string.IsNullOrWhiteSpace(SearchText);

            // すべてのカテゴリの検索結果数を更新
            foreach (var category in Categories)
            {
                category.HasSearchText = hasSearch;
                if (hasSearch)
                {
                    var categoryItems = category.Items.Select(vm => vm.Model);
                    var matchedItems = _searchService.Search(categoryItems, SearchText);
                    category.SearchMatchCount = matchedItems.Count();
                }
                else
                {
                    category.SearchMatchCount = 0;
                }
            }

            if (SelectedCategory == null) return;

            var items = SelectedCategory.Items.Select(vm => vm.Model);

            if (hasSearch)
            {
                items = _searchService.Search(items, SearchText);
                foreach (var item in items)
                {
                    var vm = SelectedCategory.Items.FirstOrDefault(x => x.Id == item.Id);
                    if (vm != null)
                    {
                        DisplayedItems.Add(vm);
                    }
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
        }

        [RelayCommand]
        private void LaunchItem(LauncherItemViewModel? item)
        {
            if (item == null) return;

            var success = _launcherService.Launch(item.Model);
            if (success)
            {
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
                RequestDeactivate?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _dialogService.ShowError($"'{item.Name}' の起動に失敗しました。");
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

            var url = _dialogService.ShowInputDialog("URLを入力してください", "URL追加", "https://");
            if (string.IsNullOrWhiteSpace(url)) return;

            // URLの簡易バリデーション
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            AddItemFromPath(url);
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
    }
}
