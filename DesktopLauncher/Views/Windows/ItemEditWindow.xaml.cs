using System.Windows;
using System.Windows.Input;
using DesktopLauncher.Infrastructure.DependencyInjection;
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
    /// ItemEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ItemEditWindow : Window
    {
        private readonly ItemEditViewModel _viewModel;

        public LauncherItem? ResultItem { get; private set; }

        public ItemEditWindow(LauncherItem? item = null)
        {
            InitializeComponent();

            var iconService = ServiceLocator.GetService<IIconService>();
            var dialogService = ServiceLocator.GetService<IDialogService>();

            _viewModel = new ItemEditViewModel(item, iconService, dialogService);
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
            if (_viewModel.Validate())
            {
                ResultItem = _viewModel.GetResultItem();
                DialogResult = true;
                Close();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
