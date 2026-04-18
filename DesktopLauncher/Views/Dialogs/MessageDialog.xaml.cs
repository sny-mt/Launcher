using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopLauncher.Views.Dialogs
{
    /// <summary>
    /// メッセージダイアログの種類
    /// </summary>
    public enum MessageDialogType
    {
        Information,
        Error
    }

    /// <summary>
    /// メッセージダイアログ
    /// </summary>
    public partial class MessageDialog : Window
    {
        public MessageDialog(string message, string title = "メッセージ", MessageDialogType type = MessageDialogType.Information)
        {
            InitializeComponent();

            TitleText.Text = title;
            Title = title;
            MessageText.Text = message;

            // タイプに応じてアイコンを変更
            if (type == MessageDialogType.Error)
            {
                IconText.Text = "!";
                IconText.Foreground = new SolidColorBrush(Color.FromRgb(232, 89, 89)); // 赤色
            }
            else
            {
                IconText.Text = "i";
                // AccentBrushを使用（デフォルト）
            }

            Loaded += (s, e) => OkButton.Focus();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
