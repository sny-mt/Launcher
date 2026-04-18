using System.Drawing;
using System.Windows;
using ColorDialog = System.Windows.Forms.ColorDialog;
using System.Windows.Input;
using DesktopLauncher.Infrastructure.DependencyInjection;
using DesktopLauncher.Infrastructure.Helpers;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Views.Windows
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public AppSettings? ResultSettings { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();

            var settingsRepository = ServiceLocator.GetService<ISettingsRepository>();
            var dataExportService = ServiceLocator.GetService<IDataExportService>();
            var dialogService = ServiceLocator.GetService<IDialogService>();
            var themeService = ServiceLocator.GetService<IThemeService>();
            _viewModel = new SettingsViewModel(settingsRepository, dataExportService, dialogService, themeService);
            DataContext = _viewModel;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveCommand.Execute(null);
            ResultSettings = _viewModel.GetCurrentSettings();
            DialogResult = true;
            Close();
        }

        private void PickBaseColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ShowColorDialog(_viewModel.CustomBaseColor);
            if (color != null) _viewModel.CustomBaseColor = color;
        }

        private void PickTextColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ShowColorDialog(_viewModel.CustomTextColor);
            if (color != null) _viewModel.CustomTextColor = color;
        }

        private void PickAccentColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ShowColorDialog(_viewModel.CustomAccentColor);
            if (color != null) _viewModel.CustomAccentColor = color;
        }

        private void EyedropBaseColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ScreenColorPicker.PickColorFromScreen(this);
            if (color != null) _viewModel.CustomBaseColor = color;
        }

        private void EyedropTextColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ScreenColorPicker.PickColorFromScreen(this);
            if (color != null) _viewModel.CustomTextColor = color;
        }

        private void EyedropAccentColor_Click(object sender, RoutedEventArgs e)
        {
            var color = ScreenColorPicker.PickColorFromScreen(this);
            if (color != null) _viewModel.CustomAccentColor = color;
        }

        private static string? ShowColorDialog(string currentHex)
        {
            try
            {
                var current = ColorTranslator.FromHtml(currentHex);
                using var dialog = new ColorDialog
                {
                    Color = current,
                    FullOpen = true
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                }
            }
            catch
            {
                // 無効なHEXの場合はデフォルトで開く
                using var dialog = new ColorDialog { FullOpen = true };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                }
            }
            return null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel.IsRecordingHotkey)
            {
                var modifiers = Keyboard.Modifiers;
                _viewModel.RecordHotkey(modifiers, e.Key);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
