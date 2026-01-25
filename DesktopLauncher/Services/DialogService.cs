using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using DesktopLauncher.Interfaces.Services;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// ダイアログサービスの実装
    /// </summary>
    public class DialogService : IDialogService
    {
        private Window? MainWindow => System.Windows.Application.Current.MainWindow;

        private T? ShowDialogWithTopmost<T>(Func<T?> showDialog)
        {
            var wasTopmost = MainWindow?.Topmost ?? false;
            if (MainWindow != null) MainWindow.Topmost = false;

            try
            {
                return showDialog();
            }
            finally
            {
                if (MainWindow != null) MainWindow.Topmost = wasTopmost;
            }
        }

        public string? ShowOpenFileDialog(string filter = "すべてのファイル|*.*")
        {
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = filter,
                    CheckFileExists = true
                };

                return dialog.ShowDialog(MainWindow) == true ? dialog.FileName : null;
            });
        }

        public string? ShowFolderBrowserDialog()
        {
            return ShowDialogWithTopmost(() =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "フォルダを選択してください",
                    ShowNewFolderButton = true
                };

                var owner = MainWindow != null
                    ? new NativeWindow(new WindowInteropHelper(MainWindow).Handle)
                    : null;

                if (dialog.ShowDialog(owner) == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
                return null;
            });
        }

        public string? ShowFileOrFolderDialog(string? initialDirectory = null)
        {
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new OpenFileDialog
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "フォルダを選択"
                };

                // 初期ディレクトリを設定
                if (!string.IsNullOrEmpty(initialDirectory))
                {
                    if (Directory.Exists(initialDirectory))
                    {
                        dialog.InitialDirectory = initialDirectory;
                    }
                    else if (File.Exists(initialDirectory))
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(initialDirectory);
                    }
                }

                if (dialog.ShowDialog() == true)
                {
                    var path = dialog.FileName;

                    // ファイルが存在する場合はそのパスを返す
                    if (File.Exists(path))
                    {
                        return path;
                    }

                    // フォルダが存在する場合はそのパスを返す
                    if (Directory.Exists(path))
                    {
                        return path;
                    }

                    // 「フォルダを選択」などのダミーファイル名の場合、親ディレクトリを返す
                    return Path.GetDirectoryName(path);
                }
                return null;
            });
        }

        public string? ShowImageFileDialog()
        {
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.ico;*.bmp|すべてのファイル|*.*",
                    CheckFileExists = true
                };

                return dialog.ShowDialog(MainWindow) == true ? dialog.FileName : null;
            });
        }

        private class NativeWindow : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; }
            public NativeWindow(IntPtr handle) => Handle = handle;
        }

        public bool ShowConfirmDialog(string message, string title = "確認")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        public void ShowMessage(string message, string title = "情報")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "エラー")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
