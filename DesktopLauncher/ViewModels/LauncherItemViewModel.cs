using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models;
using DesktopLauncher.Models.Enums;
using DesktopLauncher.ViewModels.Base;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// ランチャーアイテムのViewModel
    /// </summary>
    public partial class LauncherItemViewModel : ViewModelBase
    {
        private readonly IIconService _iconService;

        [ObservableProperty]
        private LauncherItem _model;

        [ObservableProperty]
        private ImageSource? _icon;

        public LauncherItemViewModel(LauncherItem model, IIconService iconService)
        {
            _iconService = iconService;
            _model = model;
            LoadIcon();
        }

        public string Id => Model.Id;
        public string Name => Model.Name;
        public string Path => Model.Path;
        public ItemType ItemType => Model.ItemType;
        public string CategoryId => Model.CategoryId;
        public int SortOrder => Model.SortOrder;
        public bool IsFavorite => Model.IsFavorite;
        public int LaunchCount => Model.LaunchCount;

        public int GridPosition
        {
            get => Model.GridPosition;
            set
            {
                if (Model.GridPosition != value)
                {
                    Model.GridPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public void LoadIcon()
        {
            // 1. Base64アイコンがあれば使用
            if (!string.IsNullOrEmpty(Model.IconBase64))
            {
                Icon = _iconService.LoadFromBase64(Model.IconBase64!);
                if (Icon != null) return;
            }

            // 2. カスタムアイコンパスがあれば使用（後方互換）
            if (!string.IsNullOrEmpty(Model.CustomIconPath))
            {
                Icon = _iconService.LoadCustomIcon(Model.CustomIconPath!);
                if (Icon != null) return;
            }

            // 3. パスからアイコンを取得（取得できなければデフォルトアイコン）
            Icon = _iconService.GetIconFromPath(Model.Path) ?? _iconService.GetDefaultIcon();
        }

        public void UpdateModel(LauncherItem model)
        {
            Model = model;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(ItemType));
            OnPropertyChanged(nameof(CategoryId));
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(LaunchCount));
            OnPropertyChanged(nameof(SortOrder));
            OnPropertyChanged(nameof(GridPosition));
            LoadIcon();
        }
    }
}
