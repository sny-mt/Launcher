using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLauncher.Infrastructure.Helpers;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Models;
using DesktopLauncher.Models.Enums;
using DesktopLauncher.ViewModels.Base;

namespace DesktopLauncher.ViewModels
{
    /// <summary>
    /// 設定画面のViewModel
    /// </summary>
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly AppSettings _originalSettings;

        [ObservableProperty]
        private ModifierKeys _hotkeyModifiers;

        [ObservableProperty]
        private Key _hotkeyKey;

        [ObservableProperty]
        private Theme _theme;

        [ObservableProperty]
        private ThemeColor _themeColor;

        [ObservableProperty]
        private double _windowOpacity;

        [ObservableProperty]
        private TileSize _tileSize;

        [ObservableProperty]
        private double _fontSize;

        [ObservableProperty]
        private string _fontFamily = "Yu Gothic UI";

        [ObservableProperty]
        private double _iconSize;

        [ObservableProperty]
        private bool _startWithWindows;

        [ObservableProperty]
        private bool _startMinimized;

        [ObservableProperty]
        private bool _hideAfterLaunch;

        [ObservableProperty]
        private bool _isRecordingHotkey;

        [ObservableProperty]
        private string _hotkeyDisplayText = string.Empty;

        [ObservableProperty]
        private string _selectedSettingsCategory = "外観";

        public SettingsViewModel(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            _originalSettings = settingsRepository.Get();

            // 現在の設定を読み込み
            LoadSettings(_originalSettings);
        }

        private void LoadSettings(AppSettings settings)
        {
            HotkeyModifiers = settings.HotkeyModifiers;
            HotkeyKey = settings.HotkeyKey;
            Theme = settings.Theme;
            ThemeColor = settings.ThemeColor;
            WindowOpacity = settings.WindowOpacity;
            TileSize = settings.TileSize;
            FontSize = settings.FontSize;
            FontFamily = settings.FontFamily;
            IconSize = settings.IconSize;
            StartWithWindows = settings.StartWithWindows;
            StartMinimized = settings.StartMinimized;
            HideAfterLaunch = settings.HideAfterLaunch;

            UpdateHotkeyDisplayText();
        }

        private void UpdateHotkeyDisplayText()
        {
            HotkeyDisplayText = HotkeyFormatter.Format(HotkeyModifiers, HotkeyKey);
        }

        [RelayCommand]
        private void StartRecordHotkey()
        {
            IsRecordingHotkey = true;
            HotkeyDisplayText = "キーを押してください...";
        }

        public void RecordHotkey(ModifierKeys modifiers, Key key)
        {
            if (!IsRecordingHotkey) return;

            // 修飾キーのみの場合は無視
            if (HotkeyFormatter.IsModifierKey(key))
            {
                return;
            }

            HotkeyModifiers = modifiers;
            HotkeyKey = key;
            IsRecordingHotkey = false;
            UpdateHotkeyDisplayText();
        }

        [RelayCommand]
        private void Save()
        {
            var settings = new AppSettings
            {
                HotkeyModifiers = HotkeyModifiers,
                HotkeyKey = HotkeyKey,
                Theme = Theme,
                ThemeColor = ThemeColor,
                WindowOpacity = WindowOpacity,
                TileSize = TileSize,
                FontSize = FontSize,
                FontFamily = FontFamily,
                IconSize = IconSize,
                StartWithWindows = StartWithWindows,
                StartMinimized = StartMinimized,
                HideAfterLaunch = HideAfterLaunch
            };

            _settingsRepository.Save(settings);
        }

        [RelayCommand]
        private void Cancel()
        {
            // 元の設定に戻す
            LoadSettings(_originalSettings);
        }

        public AppSettings GetCurrentSettings()
        {
            return new AppSettings
            {
                HotkeyModifiers = HotkeyModifiers,
                HotkeyKey = HotkeyKey,
                Theme = Theme,
                ThemeColor = ThemeColor,
                WindowOpacity = WindowOpacity,
                TileSize = TileSize,
                FontSize = FontSize,
                FontFamily = FontFamily,
                IconSize = IconSize,
                StartWithWindows = StartWithWindows,
                StartMinimized = StartMinimized,
                HideAfterLaunch = HideAfterLaunch
            };
        }
    }
}
