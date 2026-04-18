using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using DesktopLauncher.Infrastructure.DependencyInjection;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private const string MutexName = "DesktopLauncher_SingleInstance_Mutex";
        private static Mutex? _mutex;

        private IHotkeyService? _hotkeyService;
        private IThemeService? _themeService;
        private IStartupService? _startupService;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        private const int SW_RESTORE = 9;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // シングルインスタンスチェック
            _mutex = new Mutex(true, MutexName, out bool createdNew);

            if (!createdNew)
            {
                // 既に起動中のインスタンスがある場合
                // 既存のウィンドウをアクティブにして終了
                ActivateExistingWindow();
                Shutdown();
                return;
            }

            // DIコンテナを初期化
            ServiceLocator.Initialize();

            // サービスを取得
            _themeService = ServiceLocator.GetService<IThemeService>();
            _startupService = ServiceLocator.GetService<IStartupService>();

            // 保存されているテーマを適用
            var settingsRepository = ServiceLocator.GetService<Interfaces.Repositories.ISettingsRepository>();
            var settings = settingsRepository.Get();
            if (settings.Theme == Models.Enums.Theme.Custom)
            {
                _themeService.ApplyCustomTheme(settings.CustomBaseColor, settings.CustomTextColor, settings.CustomAccentColor);
            }
            else
            {
                ApplyTheme(settings.Theme);
                ApplyThemeColor(settings.ThemeColor);
            }
            ApplyWindowOpacity(settings.WindowOpacity);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // ホットキーサービスを破棄
            _hotkeyService?.Dispose();

            // Mutexを解放
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
                _mutex = null;
            }
        }

        private static void ActivateExistingWindow()
        {
            // 既存のウィンドウを探してアクティブにする
            var hWnd = FindWindow(null, "Desktop Launcher");
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
        }

        public void SetHotkeyService(IHotkeyService hotkeyService)
        {
            _hotkeyService = hotkeyService;
        }

        public void ApplyTheme(Theme theme)
        {
            _themeService?.ApplyTheme(theme);
        }

        public void ApplyThemeColor(ThemeColor themeColor)
        {
            _themeService?.ApplyThemeColor(themeColor);
        }

        public void ApplyWindowOpacity(double transparency)
        {
            _themeService?.ApplyWindowOpacity(transparency);
        }

        public void SetStartWithWindows(bool enable)
        {
            _startupService?.SetEnabled(enable);
        }
    }
}
