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
        private IHotkeyService? _hotkeyService;
        private IThemeService? _themeService;
        private IStartupService? _startupService;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // DIコンテナを初期化
            ServiceLocator.Initialize();

            // サービスを取得
            _themeService = ServiceLocator.GetService<IThemeService>();
            _startupService = ServiceLocator.GetService<IStartupService>();

            // 保存されているテーマを適用
            var settingsRepository = ServiceLocator.GetService<Interfaces.Repositories.ISettingsRepository>();
            var settings = settingsRepository.Get();
            ApplyTheme(settings.Theme);
            ApplyThemeColor(settings.ThemeColor);
            ApplyWindowOpacity(settings.WindowOpacity);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // ホットキーサービスを破棄
            _hotkeyService?.Dispose();
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
