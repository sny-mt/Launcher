using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Infrastructure.Helpers
{
    /// <summary>
    /// ドラッグ&ドロップ操作のヘルパークラス
    /// </summary>
    public class DragDropHelper
    {
        private Point _dragStartPoint;
        private LauncherItemViewModel? _draggedItem;
        private CategoryViewModel? _draggedCategory;

        /// <summary>
        /// ドラッグ中のアイテム
        /// </summary>
        public LauncherItemViewModel? DraggedItem => _draggedItem;

        /// <summary>
        /// ドラッグ中のカテゴリ
        /// </summary>
        public CategoryViewModel? DraggedCategory => _draggedCategory;

        /// <summary>
        /// タイルボタンのマウスダウン処理
        /// </summary>
        public void HandleTileMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);

            if (sender is Button button)
            {
                if (button.DataContext is GridSlotViewModel slot && slot.Item != null)
                {
                    _draggedItem = slot.Item;
                }
                else if (button.DataContext is LauncherItemViewModel item)
                {
                    _draggedItem = item;
                }
            }
        }

        /// <summary>
        /// タイルボタンのマウスムーブ処理
        /// </summary>
        /// <returns>ドラッグが開始された場合はtrue</returns>
        public bool HandleTileMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggedItem == null)
                return false;

            var currentPos = e.GetPosition(null);
            var diff = _dragStartPoint - currentPos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is Button button)
                {
                    var data = new DataObject("LauncherItem", _draggedItem);
                    DragDrop.DoDragDrop(button, data, DragDropEffects.Move);
                    _draggedItem = null;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// タイルボタンのマウスアップ処理
        /// </summary>
        public void HandleTileMouseUp()
        {
            _draggedItem = null;
        }

        /// <summary>
        /// タブアイテムのマウスダウン処理
        /// </summary>
        public void HandleTabMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);

            if (sender is TabItem tabItem && tabItem.DataContext is CategoryViewModel category)
            {
                _draggedCategory = category;
            }
        }

        /// <summary>
        /// タブアイテムのマウスムーブ処理
        /// </summary>
        /// <returns>ドラッグが開始された場合はtrue</returns>
        public bool HandleTabMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _draggedCategory == null)
                return false;

            var currentPos = e.GetPosition(null);
            var diff = _dragStartPoint - currentPos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (sender is TabItem tabItem)
                {
                    var data = new DataObject("Category", _draggedCategory);
                    DragDrop.DoDragDrop(tabItem, data, DragDropEffects.Move);
                    _draggedCategory = null;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// グリッドスロットのハイライト切り替え
        /// </summary>
        public static void HighlightSlot(Border? border, bool highlight, FrameworkElement resourceSource)
        {
            if (border == null) return;

            if (highlight)
            {
                border.BorderBrush = (Brush)resourceSource.FindResource("AccentBrush");
                border.BorderThickness = new Thickness(2);
            }
            else
            {
                border.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
                border.BorderThickness = new Thickness(1);
            }
        }

        /// <summary>
        /// タブアイテムのハイライト切り替え
        /// </summary>
        public static void HighlightTabItem(TabItem? tabItem, bool highlight, FrameworkElement resourceSource)
        {
            if (tabItem == null) return;

            if (highlight)
            {
                tabItem.BorderBrush = (Brush)resourceSource.FindResource("AccentBrush");
                tabItem.BorderThickness = new Thickness(2);
            }
            else
            {
                tabItem.SetResourceReference(TabItem.BorderBrushProperty, "BorderBrush");
                tabItem.BorderThickness = new Thickness(0);
            }
        }

        /// <summary>
        /// ドラッグオーバー時の効果を判定
        /// </summary>
        public static DragDropEffects GetDragEffects(DragEventArgs e, Func<string, bool> isUrlChecker)
        {
            // 内部アイテム移動
            if (e.Data.GetDataPresent("LauncherItem"))
            {
                return DragDropEffects.Move;
            }

            // カテゴリ移動
            if (e.Data.GetDataPresent("Category"))
            {
                return DragDropEffects.Move;
            }

            // 外部ファイルドロップ
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return DragDropEffects.Copy;
            }

            // URLドロップ
            if (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                var text = e.Data.GetData(DataFormats.UnicodeText) as string
                        ?? e.Data.GetData(DataFormats.Text) as string;
                if (!string.IsNullOrEmpty(text) && isUrlChecker(text!))
                {
                    return DragDropEffects.Copy;
                }
            }

            return DragDropEffects.None;
        }

        /// <summary>
        /// ドロップデータからテキスト（URL）を取得
        /// </summary>
        public static string? GetDroppedText(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                return e.Data.GetData(DataFormats.UnicodeText) as string;
            }
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                return e.Data.GetData(DataFormats.Text) as string;
            }
            return null;
        }

        /// <summary>
        /// ドロップデータからファイルパスを取得
        /// </summary>
        public static string[]? GetDroppedFiles(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return e.Data.GetData(DataFormats.FileDrop) as string[];
            }
            return null;
        }

        /// <summary>
        /// ドロップデータからアイテムを取得
        /// </summary>
        public static LauncherItemViewModel? GetDroppedItem(DragEventArgs e)
        {
            if (e.Data.GetDataPresent("LauncherItem"))
            {
                return e.Data.GetData("LauncherItem") as LauncherItemViewModel;
            }
            return null;
        }

        /// <summary>
        /// ドロップデータからカテゴリを取得
        /// </summary>
        public static CategoryViewModel? GetDroppedCategory(DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Category"))
            {
                return e.Data.GetData("Category") as CategoryViewModel;
            }
            return null;
        }
    }
}
