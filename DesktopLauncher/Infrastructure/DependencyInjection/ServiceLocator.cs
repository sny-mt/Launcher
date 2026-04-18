using System;
using Microsoft.Extensions.DependencyInjection;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Repositories;
using DesktopLauncher.Services;
using DesktopLauncher.Services.Icons;
using DesktopLauncher.Services.Data;
using DesktopLauncher.Services.Operations;
using DesktopLauncher.Services.Ui;
using DesktopLauncher.Services.Shell;
using DesktopLauncher.Services.Search;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Infrastructure.DependencyInjection
{
    /// <summary>
    /// DIコンテナのセットアップとサービス取得
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// サービスプロバイダー
        /// </summary>
        public static IServiceProvider ServiceProvider
        {
            get => _serviceProvider ?? throw new InvalidOperationException("ServiceLocator has not been initialized.");
            private set => _serviceProvider = value;
        }

        /// <summary>
        /// DIコンテナを初期化
        /// </summary>
        public static void Initialize()
        {
            var services = new ServiceCollection();

            // データストア（シングルトン）
            services.AddSingleton<JsonDataStore>();

            // リポジトリ（シングルトン）
            services.AddSingleton<IItemRepository, ItemRepository>();
            services.AddSingleton<ICategoryRepository, CategoryRepository>();
            services.AddSingleton<ISettingsRepository, SettingsRepository>();

            // 基盤サービス（シングルトン）
            services.AddSingleton<IHotkeyService, HotkeyService>();
            services.AddSingleton<IPathIconService, PathIconService>();
            services.AddSingleton<IImageCodecService, ImageCodecService>();
            services.AddSingleton<IFaviconService, FaviconService>();
            services.AddSingleton<IIconService, IconService>();
            services.AddSingleton<ILauncherService, LauncherService>();
            services.AddSingleton<ISearchService, SearchService>();
            services.AddSingleton<IDialogService, DialogService>();

            // 新規サービス（シングルトン）
            services.AddSingleton<IItemTypeDetectionService, ItemTypeDetectionService>();
            services.AddSingleton<IGridPositioningService, GridPositioningService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IItemOperationsService, ItemOperationsService>();
            services.AddSingleton<ICategoryOperationsService, CategoryOperationsService>();
            services.AddSingleton<IDataExportService, DataExportService>();

            // ViewModels
            services.AddTransient<MainViewModel>();

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// サービスを取得
        /// </summary>
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// サービスを取得（取得できない場合はnull）
        /// </summary>
        public static T? GetServiceOrDefault<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }
    }
}
