using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopLauncher.Infrastructure.DependencyInjection;
using DesktopLauncher.Infrastructure.Helpers;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly IHotkeyService _hotkeyService;
        private readonly IItemTypeDetectionService _itemTypeDetectionService;
        private readonly DragDropHelper _dragDropHelper;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = ServiceLocator.GetService<MainViewModel>();
            _viewModel.SettingsChanged += OnSettingsChanged;
            _viewModel.RequestHideWindow += OnRequestHideWindow;
            _viewModel.RequestDeactivate += OnRequestDeactivate;
            DataContext = _viewModel;

            _hotkeyService = ServiceLocator.GetService<IHotkeyService>();
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            _itemTypeDetectionService = ServiceLocator.GetService<IItemTypeDetectionService>();
            _dragDropHelper = new DragDropHelper();

            // アプリにホットキーサービスを登録
            ((App)Application.Current).SetHotkeyService(_hotkeyService);

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ホットキーを登録
            var settings = _viewModel.Settings;
            if (!_hotkeyService.Register(settings.HotkeyModifiers, settings.HotkeyKey))
            {
                MessageBox.Show(
                    "ホットキーの登録に失敗しました。\n他のアプリケーションで使用されている可能性があります。",
                    "警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // 前面固定ボタンの初期状態を設定
            TopmostButton.ToolTip = Topmost ? "前面固定 (ON)" : "前面固定 (OFF)";
            TopmostButton.Opacity = Topmost ? 1.0 : 0.5;

            // 検索ボックスにフォーカス
            SearchBox.Focus();
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsVisible)
                {
                    HideWindow();
                }
                else
                {
                    ShowWindow();
                }
            });
        }

        private void OnRequestHideWindow(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => HideWindow());
        }

        private void OnRequestDeactivate(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // 最前面を解除して後ろに回る
                Topmost = false;
                TopmostButton.ToolTip = "前面固定 (OFF)";
                TopmostButton.Opacity = 0.5;
            });
        }

        private void ShowWindow()
        {
            Show();
            Activate();
            SearchBox.Focus();
            SearchBox.SelectAll();
        }

        private void HideWindow()
        {
            Hide();
            _viewModel.SearchText = string.Empty;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var files = DragDropHelper.GetDroppedFiles(e);
            if (files != null && files.Any())
            {
                _viewModel.HandleFileDrop(files);
                return;
            }

            var text = DragDropHelper.GetDroppedText(e);
            if (!string.IsNullOrEmpty(text) && _itemTypeDetectionService.IsUrl(text!))
            {
                _viewModel.AddItemFromPath(text!);
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropHelper.GetDragEffects(e, _itemTypeDetectionService.IsUrl);
            e.Handled = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // ウィンドウがフォーカスを失ったら隠す（オプション）
            // HideWindow();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var hasSearchText = !string.IsNullOrEmpty(_viewModel.SearchText);
            var hasSlotSelection = _viewModel.SelectedSlotIndex >= 0;

            // 検索中で左右キー: スロット選択がなければテキスト編集用に使用
            var allowLeftRight = !hasSearchText || hasSlotSelection;

            if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Ctrl+E: 検索ボックスにフォーカス
                _viewModel.ResetSelection();
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // 検索中でスロット選択がある場合は選択解除
                if (hasSearchText && hasSlotSelection)
                {
                    _viewModel.ResetSelection();
                    SearchBox.Focus();
                    e.Handled = true;
                }
                else
                {
                    HideWindow();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Tab)
            {
                // Tab: 次のカテゴリ、Shift+Tab: 前のカテゴリ
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    _viewModel.SelectPreviousCategory();
                }
                else
                {
                    _viewModel.SelectNextCategory();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Left && allowLeftRight)
            {
                _viewModel.NavigateSlot(-1, 0);
                e.Handled = true;
            }
            else if (e.Key == Key.Right && allowLeftRight)
            {
                _viewModel.NavigateSlot(1, 0);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                _viewModel.NavigateSlot(0, -1);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                _viewModel.NavigateSlot(0, 1);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                // 選択中のアイテムを起動
                var selectedItem = _viewModel.GetSelectedItem();
                if (selectedItem != null)
                {
                    _viewModel.LaunchItemCommand.Execute(selectedItem);
                    e.Handled = true;
                }
                else if (!hasSlotSelection && _viewModel.DisplayedItems.Any())
                {
                    // スロット選択がなく、表示アイテムがある場合のみ最初のアイテムを起動
                    var firstItem = _viewModel.DisplayedItems.First();
                    _viewModel.LaunchItemCommand.Execute(firstItem);
                    e.Handled = true;
                }
            }
        }

        private void OnSettingsChanged(object? sender, AppSettings settings)
        {
            var app = (App)Application.Current;

            // ホットキーを再登録
            _hotkeyService.Unregister();
            if (!_hotkeyService.Register(settings.HotkeyModifiers, settings.HotkeyKey))
            {
                MessageBox.Show(
                    "ホットキーの登録に失敗しました。\n他のアプリケーションで使用されている可能性があります。",
                    "警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // テーマを適用
            app.ApplyTheme(settings.Theme);
            app.ApplyThemeColor(settings.ThemeColor);
            app.ApplyWindowOpacity(settings.WindowOpacity);

            // スタートアップ登録
            app.SetStartWithWindows(settings.StartWithWindows);
        }

        private void CategoryScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // マウスホイールで横スクロール
            if (sender is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            TopmostButton.ToolTip = Topmost ? "前面固定 (ON)" : "前面固定 (OFF)";
            TopmostButton.Opacity = Topmost ? 1.0 : 0.5;
        }

        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 操作可能な要素をクリックした場合はドラッグしない
            var source = e.OriginalSource as DependencyObject;
            while (source != null)
            {
                if (source is System.Windows.Controls.Primitives.ButtonBase ||
                    source is TextBox ||
                    source is System.Windows.Controls.Primitives.Thumb ||
                    source is TabItem)
                {
                    return;
                }

                // Run等のTextElementはVisualではないため、LogicalTreeを使用
                if (source is Visual)
                {
                    source = VisualTreeHelper.GetParent(source);
                }
                else
                {
                    source = LogicalTreeHelper.GetParent(source);
                }
            }

            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        #region タイルのドラッグ&ドロップ

        private void TileButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragDropHelper.HandleTileMouseDown(sender, e);
        }

        private void TileButton_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _dragDropHelper.HandleTileMouseMove(sender, e);
        }

        private void TileButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _dragDropHelper.HandleTileMouseUp();
        }

        private void GridSlot_DragOver(object sender, DragEventArgs e)
        {
            var effects = DragDropHelper.GetDragEffects(e, _itemTypeDetectionService.IsUrl);
            e.Effects = effects;

            if (effects != DragDropEffects.None)
            {
                DragDropHelper.HighlightSlot(sender as Border, true, this);
            }
            e.Handled = true;
        }

        private void GridSlot_DragLeave(object sender, DragEventArgs e)
        {
            DragDropHelper.HighlightSlot(sender as Border, false, this);
        }

        private void GridSlot_Drop(object sender, DragEventArgs e)
        {
            DragDropHelper.HighlightSlot(sender as Border, false, this);

            var targetBorder = sender as Border;
            var slotIndex = targetBorder?.Tag as int? ?? -1;
            if (slotIndex < 0) return;

            // 内部アイテム移動
            var draggedItem = DragDropHelper.GetDroppedItem(e);
            if (draggedItem != null)
            {
                _viewModel.MoveItemToSlot(draggedItem, slotIndex);
                e.Handled = true;
                return;
            }

            // 外部ファイルドロップ
            var files = DragDropHelper.GetDroppedFiles(e);
            if (files != null && files.Any())
            {
                _viewModel.AddItemsToSlot(files, slotIndex);
                e.Handled = true;
                return;
            }

            // URLドロップ
            var text = DragDropHelper.GetDroppedText(e);
            if (!string.IsNullOrEmpty(text) && _itemTypeDetectionService.IsUrl(text!))
            {
                _viewModel.AddItemToSlot(text!, slotIndex);
                e.Handled = true;
            }
        }

        #endregion

        #region タブ（カテゴリ）のドラッグ&ドロップ

        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragDropHelper.HandleTabMouseDown(sender, e);
        }

        private void TabItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabItem tabItem && tabItem.DataContext is CategoryViewModel category)
            {
                _viewModel.EditCategoryCommand.Execute(category);
                e.Handled = true;
            }
        }

        private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _dragDropHelper.HandleTabMouseMove(sender, e);
        }

        private void TabItem_DragOver(object sender, DragEventArgs e)
        {
            // カテゴリの並べ替えまたはアイテムの移動
            if (e.Data.GetDataPresent("Category") || e.Data.GetDataPresent("LauncherItem"))
            {
                e.Effects = DragDropEffects.Move;
                DragDropHelper.HighlightTabItem(sender as TabItem, true, this);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void TabItem_DragLeave(object sender, DragEventArgs e)
        {
            DragDropHelper.HighlightTabItem(sender as TabItem, false, this);
        }

        private void TabItem_Drop(object sender, DragEventArgs e)
        {
            DragDropHelper.HighlightTabItem(sender as TabItem, false, this);

            var targetTabItem = sender as TabItem;
            var targetCategory = targetTabItem?.DataContext as CategoryViewModel;
            if (targetCategory == null) return;

            // カテゴリの並べ替え
            var draggedCategory = DragDropHelper.GetDroppedCategory(e);
            if (draggedCategory != null && draggedCategory != targetCategory)
            {
                var targetIndex = _viewModel.Categories.IndexOf(targetCategory);
                _viewModel.MoveCategory(draggedCategory, targetIndex);
                e.Handled = true;
                return;
            }

            // アイテムを別カテゴリに移動
            var draggedItem = DragDropHelper.GetDroppedItem(e);
            if (draggedItem != null)
            {
                _viewModel.MoveItemToCategory(draggedItem, targetCategory);
                e.Handled = true;
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.SettingsChanged -= OnSettingsChanged;
            _viewModel.RequestHideWindow -= OnRequestHideWindow;
            _viewModel.RequestDeactivate -= OnRequestDeactivate;
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
            base.OnClosed(e);
        }
    }
}
