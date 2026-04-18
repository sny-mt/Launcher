using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLauncher.Infrastructure.Helpers;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models;
using DesktopLauncher.Models.Enums;
using DesktopLauncher.Services;
using DesktopLauncher.Services.Icons;
using DesktopLauncher.Services.Data;
using DesktopLauncher.Services.Operations;
using DesktopLauncher.Services.Ui;
using DesktopLauncher.Services.Shell;
using DesktopLauncher.Services.Search;
using DesktopLauncher.ViewModels.Base;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// アイテム編集画面のViewModel
    /// </summary>
    public partial class ItemEditViewModel : ViewModelBase
    {
        private readonly IIconService _iconService;
        private readonly IDialogService _dialogService;
        private readonly LauncherItem _originalItem;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _path = string.Empty;

        [ObservableProperty]
        private ItemType _itemType;

        [ObservableProperty]
        private string? _customIconPath;

        [ObservableProperty]
        private string? _iconBase64;

        [ObservableProperty]
        private string? _arguments;

        [ObservableProperty]
        private string? _workingDirectory;

        [ObservableProperty]
        private ImageSource? _previewIcon;

        public bool IsNewItem { get; }

        public ItemEditViewModel(
            LauncherItem? item,
            IIconService iconService,
            IDialogService dialogService)
        {
            _iconService = iconService;
            _dialogService = dialogService;

            if (item == null)
            {
                IsNewItem = true;
                _originalItem = new LauncherItem();
            }
            else
            {
                IsNewItem = false;
                _originalItem = item;
                LoadFromItem(item);
            }

            UpdatePreviewIcon();
        }

        private void LoadFromItem(LauncherItem item)
        {
            Name = item.Name;
            Path = item.Path;
            ItemType = item.ItemType;
            CustomIconPath = item.CustomIconPath;
            IconBase64 = item.IconBase64;
            Arguments = item.Arguments;
            WorkingDirectory = item.WorkingDirectory;
        }

        partial void OnPathChanged(string value)
        {
            // URLの場合は自動的にItemTypeを設定
            if (UrlHelper.IsUrl(value))
            {
                ItemType = ItemType.Url;
            }
            UpdatePreviewIcon();
        }

        partial void OnCustomIconPathChanged(string? value)
        {
            UpdatePreviewIcon();
        }

        private void UpdatePreviewIcon()
        {
            // カスタムアイコンパスがあれば使用
            if (!string.IsNullOrEmpty(CustomIconPath))
            {
                PreviewIcon = _iconService.LoadCustomIcon(CustomIconPath!);
                if (PreviewIcon != null) return;
            }

            // Base64アイコンがあれば使用
            if (!string.IsNullOrEmpty(IconBase64))
            {
                PreviewIcon = _iconService.LoadFromBase64(IconBase64!);
                if (PreviewIcon != null) return;
            }

            // パスからアイコンを取得
            PreviewIcon = _iconService.GetIconFromPath(Path);
        }

        [RelayCommand]
        private void BrowsePath()
        {
            var path = _dialogService.ShowFileOrFolderDialog(Path);
            if (!string.IsNullOrEmpty(path))
            {
                Path = path!;

                // フォルダかファイルかを判定
                if (System.IO.Directory.Exists(path))
                {
                    ItemType = ItemType.Folder;
                    if (string.IsNullOrEmpty(Name))
                    {
                        Name = new System.IO.DirectoryInfo(path).Name;
                    }
                }
                else if (string.IsNullOrEmpty(Name))
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(path);
                }
            }
        }

        [RelayCommand]
        private void BrowseFolder()
        {
            var path = _dialogService.ShowFolderBrowserDialog();
            if (!string.IsNullOrEmpty(path))
            {
                Path = path!;
                ItemType = ItemType.Folder;

                // 名前が空の場合は自動設定
                if (string.IsNullOrEmpty(Name))
                {
                    Name = new System.IO.DirectoryInfo(path).Name;
                }
            }
        }

        [RelayCommand]
        private void BrowseIcon()
        {
            var path = _dialogService.ShowImageFileDialog();
            if (!string.IsNullOrEmpty(path))
            {
                CustomIconPath = path;
            }
        }

        [RelayCommand]
        private void ClearIcon()
        {
            CustomIconPath = null;
        }

        [RelayCommand]
        private void BrowseWorkingDirectory()
        {
            var path = _dialogService.ShowFolderBrowserDialog();
            if (!string.IsNullOrEmpty(path))
            {
                WorkingDirectory = path;
            }
        }

        public LauncherItem GetResultItem()
        {
            // アイコンをBase64に変換
            string? iconBase64 = null;

            // カスタムアイコンパスがあれば変換
            if (!string.IsNullOrEmpty(CustomIconPath))
            {
                var customIcon = _iconService.LoadCustomIcon(CustomIconPath!);
                if (customIcon != null)
                {
                    iconBase64 = _iconService.ConvertToBase64(customIcon);
                }
            }

            // カスタムアイコンがない場合はパスから取得
            if (string.IsNullOrEmpty(iconBase64))
            {
                if (UrlHelper.IsUrl(Path))
                {
                    iconBase64 = _iconService.DownloadFaviconAsBase64(Path);
                }
                else
                {
                    iconBase64 = _iconService.GetIconBase64FromPath(Path);
                }
            }

            return new LauncherItem
            {
                Id = _originalItem.Id,
                Name = Name,
                Path = Path,
                ItemType = ItemType,
                CategoryId = _originalItem.CategoryId,
                CustomIconPath = null, // パスは使わない
                IconBase64 = iconBase64,
                Arguments = Arguments,
                WorkingDirectory = WorkingDirectory,
                SortOrder = _originalItem.SortOrder,
                GridPosition = _originalItem.GridPosition,
                CreatedAt = _originalItem.CreatedAt
            };
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                _dialogService.ShowError("名前を入力してください。");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Path))
            {
                _dialogService.ShowError("パスを入力してください。");
                return false;
            }

            return true;
        }

    }
}
