using System.Windows;
using System.Windows.Input;

namespace DesktopLauncher.Views
{
    /// <summary>
    /// 汎用テキスト入力ダイアログ
    /// </summary>
    public partial class InputDialog : Window
    {
        public string InputText => InputTextBox.Text;

        public InputDialog(string prompt, string title = "入力", string defaultValue = "")
        {
            InitializeComponent();

            TitleText.Text = title;
            Title = title;
            PromptText.Text = prompt;
            InputTextBox.Text = defaultValue;

            Loaded += (s, e) =>
            {
                InputTextBox.Focus();
                InputTextBox.SelectAll();
            };
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
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
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
