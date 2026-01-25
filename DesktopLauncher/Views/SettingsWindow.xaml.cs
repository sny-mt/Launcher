using System.Windows;
using System.Windows.Input;
using DesktopLauncher.Infrastructure.DependencyInjection;
using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Models;
using DesktopLauncher.ViewModels;

namespace DesktopLauncher.Views
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
            _viewModel = new SettingsViewModel(settingsRepository);
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
