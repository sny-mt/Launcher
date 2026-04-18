using System;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopLauncher.ViewModels.Base;

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
    public partial class ToastViewModel : ViewModelBase
    {
        private const int CloseAnimationDurationMs = 300;

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
            _timer.Tick += OnAutoCloseTimerTick;
            _timer.Start();
        }

        private void OnAutoCloseTimerTick(object? sender, EventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= OnAutoCloseTimerTick;
            Close();
        }

        public void Close()
        {
            if (IsClosing) return;

            IsClosing = true;
            // アニメーション完了後にCloseRequestedを発火
            var closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(CloseAnimationDurationMs)
            };

            void OnCloseAnimationTick(object? s, EventArgs args)
            {
                closeTimer.Stop();
                closeTimer.Tick -= OnCloseAnimationTick;
                IsVisible = false;
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }

            closeTimer.Tick += OnCloseAnimationTick;
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
