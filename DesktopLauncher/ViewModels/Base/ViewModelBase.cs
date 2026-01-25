using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLauncher.ViewModels.Base
{
    /// <summary>
    /// ViewModel基底クラス
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isBusy;

        /// <summary>
        /// 処理中フラグ
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
