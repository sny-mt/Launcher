using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopLauncher.Infrastructure.Helpers;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Interfaces.Services;
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
        private readonly IDataExportService? _dataExportService;
        private readonly IDialogService? _dialogService;
        private readonly IThemeService? _themeService;
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

        [ObservableProperty]
        private string _exportImportMessage = string.Empty;

        [ObservableProperty]
        private string _customBaseColor = "#1E1E1E";

        [ObservableProperty]
        private string _customTextColor = "#FFFFFF";

        [ObservableProperty]
        private string _customAccentColor = "#0078D4";

        public bool IsCustomTheme => Theme == Theme.Custom;

        public event EventHandler? DataImported;

        public SettingsViewModel(
            ISettingsRepository settingsRepository,
            IDataExportService? dataExportService = null,
            IDialogService? dialogService = null,
            IThemeService? themeService = null)
        {
            _settingsRepository = settingsRepository;
            _dataExportService = dataExportService;
            _dialogService = dialogService;
            _themeService = themeService;
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
            CustomBaseColor = settings.CustomBaseColor;
            CustomTextColor = settings.CustomTextColor;
            CustomAccentColor = settings.CustomAccentColor;

            UpdateHotkeyDisplayText();
        }

        partial void OnThemeChanged(Theme value)
        {
            OnPropertyChanged(nameof(IsCustomTheme));
            if (value == Theme.Custom)
            {
                ApplyCustomThemePreview();
            }
        }

        partial void OnCustomBaseColorChanged(string value) => ApplyCustomThemePreview();
        partial void OnCustomTextColorChanged(string value) => ApplyCustomThemePreview();
        partial void OnCustomAccentColorChanged(string value) => ApplyCustomThemePreview();

        private void ApplyCustomThemePreview()
        {
            if (Theme != Theme.Custom || _themeService == null) return;
            _themeService.ApplyCustomTheme(CustomBaseColor, CustomTextColor, CustomAccentColor);
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
            _settingsRepository.Save(GetCurrentSettings());
        }

        [RelayCommand]
        private void Cancel()
        {
            // 元の設定に戻す
            LoadSettings(_originalSettings);
        }

        [RelayCommand]
        private void ExportData()
        {
            if (_dataExportService == null || _dialogService == null) return;

            var filePath = _dialogService.ShowSaveFileDialog(
                "データをエクスポート",
                "JSON Files (*.json)|*.json",
                "launcher_data_backup.json");

            if (string.IsNullOrEmpty(filePath)) return;

            ExportImportMessage = _dataExportService.ExportToFile(filePath!)
                ? "エクスポートが完了しました。"
                : "エクスポートに失敗しました。";
        }

        [RelayCommand]
        private void ImportData()
        {
            if (_dataExportService == null || _dialogService == null) return;

            if (!_dialogService.ShowConfirmDialog("現在のデータを上書きしてインポートしますか？\nこの操作は元に戻せません。"))
            {
                return;
            }

            var filePath = _dialogService.ShowOpenFileDialog(
                "データをインポート",
                "JSON Files (*.json)|*.json");

            if (string.IsNullOrEmpty(filePath)) return;

            if (_dataExportService.ImportFromFile(filePath!))
            {
                ExportImportMessage = "インポートが完了しました。アプリを再起動してください。";
                DataImported?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ExportImportMessage = "インポートに失敗しました。ファイル形式を確認してください。";
            }
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
                HideAfterLaunch = HideAfterLaunch,
                CustomBaseColor = CustomBaseColor,
                CustomTextColor = CustomTextColor,
                CustomAccentColor = CustomAccentColor
            };
        }
    }
}
