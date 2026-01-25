using System.Windows;
using System.Windows.Input;

namespace DesktopLauncher.Views
{
    /// <summary>
    /// 確認ダイアログ
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string message, string title = "確認")
        {
            InitializeComponent();

            TitleText.Text = title;
            Title = title;
            MessageText.Text = message;

            Loaded += (s, e) => YesButton.Focus();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
