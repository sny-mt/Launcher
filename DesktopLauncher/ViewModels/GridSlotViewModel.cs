using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLauncher.ViewModels.Base;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// グリッドスロットのViewModel
    /// </summary>
    public partial class GridSlotViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int _slotIndex;

        [ObservableProperty]
        private LauncherItemViewModel? _item;

        [ObservableProperty]
        private bool _isSelected;

        public bool HasItem => Item != null;

        partial void OnItemChanged(LauncherItemViewModel? value)
        {
            OnPropertyChanged(nameof(HasItem));
        }
    }
}
