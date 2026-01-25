using System.Windows;
using System.Windows.Input;
using DesktopLauncher.Models;

namespace DesktopLauncher.Views
{
    /// <summary>
    /// CategoryEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CategoryEditWindow : Window
    {
        public string CategoryName { get; set; } = string.Empty;
        public Category? ResultCategory { get; private set; }

        private readonly Category? _editingCategory;

        public CategoryEditWindow(Category? category = null)
        {
            InitializeComponent();
            DataContext = this;

            _editingCategory = category;

            if (category != null)
            {
                CategoryName = category.Name;
                TitleText.Text = "カテゴリを編集";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryNameTextBox.Focus();
            CategoryNameTextBox.SelectAll();
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
            var name = CategoryNameTextBox.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("カテゴリ名を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryNameTextBox.Focus();
                return;
            }

            if (_editingCategory != null)
            {
                ResultCategory = new Category
                {
                    Id = _editingCategory.Id,
                    Name = name!,
                    SortOrder = _editingCategory.SortOrder,
                    Icon = _editingCategory.Icon,
                    CreatedAt = _editingCategory.CreatedAt
                };
            }
            else
            {
                ResultCategory = new Category
                {
                    Name = name!
                };
            }

            DialogResult = true;
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
                SaveButton_Click(sender, e);
            }
        }
    }
}
