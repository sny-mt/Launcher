using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLauncher.Models;
using DesktopLauncher.ViewModels.Base;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// カテゴリのViewModel
    /// </summary>
    public partial class CategoryViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Category _model;

        [ObservableProperty]
        private ObservableCollection<LauncherItemViewModel> _items;

        [ObservableProperty]
        private int _searchMatchCount;

        [ObservableProperty]
        private bool _hasSearchText;

        public CategoryViewModel(Category model)
        {
            _model = model;
            _items = new ObservableCollection<LauncherItemViewModel>();
        }

        public string Id => Model.Id;
        public string Name => Model.Name;
        public int SortOrder => Model.SortOrder;
        public string? Icon => Model.Icon;

        public void UpdateModel(Category model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            Model = model;
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(SortOrder));
            OnPropertyChanged(nameof(Icon));
        }
    }
}
