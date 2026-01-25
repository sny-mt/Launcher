using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// トースト通知の種類
    /// </summary>
    public enum ToastType
    {
        Success,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// トースト通知のViewModel
    /// </summary>
    public partial class ToastViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private ToastType _type = ToastType.Info;

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private bool _isClosing = false;

        public event EventHandler? CloseRequested;

        private readonly DispatcherTimer _timer;

        public ToastViewModel(string message, ToastType type = ToastType.Success, int durationMs = 3000)
        {
            Message = message;
            Type = type;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMs)
            };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                Close();
            };
            _timer.Start();
        }

        public void Close()
        {
            IsClosing = true;
            // アニメーション完了後にCloseRequestedを発火
            var closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                IsVisible = false;
                CloseRequested?.Invoke(this, EventArgs.Empty);
            };
            closeTimer.Start();
        }

        public string IconText => Type switch
        {
            ToastType.Success => "✓",
            ToastType.Info => "i",
            ToastType.Warning => "!",
            ToastType.Error => "✕",
            _ => "i"
        };
    }
}
