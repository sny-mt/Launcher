using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Views.Dialogs;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DesktopLauncher.Services.Ui
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
                        var dir = Path.GetDirectoryName(initialDirectory);
                        if (dir != null)
                        {
                            dialog.InitialDirectory = dir;
                        }
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
                    return Path.GetDirectoryName(path) ?? null;
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
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new ConfirmDialog(message, title)
                {
                    Owner = MainWindow
                };

                return dialog.ShowDialog() == true;
            });
        }

        public void ShowMessage(string message, string title = "情報")
        {
            ShowDialogWithTopmost(() =>
            {
                var dialog = new MessageDialog(message, title, MessageDialogType.Information)
                {
                    Owner = MainWindow
                };

                dialog.ShowDialog();
                return true;
            });
        }

        public void ShowError(string message, string title = "エラー")
        {
            ShowDialogWithTopmost(() =>
            {
                var dialog = new MessageDialog(message, title, MessageDialogType.Error)
                {
                    Owner = MainWindow
                };

                dialog.ShowDialog();
                return true;
            });
        }

        public string? ShowInputDialog(string prompt, string title = "入力", string defaultValue = "")
        {
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new InputDialog(prompt, title, defaultValue)
                {
                    Owner = MainWindow
                };

                return dialog.ShowDialog() == true ? dialog.InputText : null;
            });
        }

        public string? ShowOpenFileDialog(string title, string filter)
        {
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new OpenFileDialog
                {
                    Title = title,
                    Filter = filter,
                    CheckFileExists = true
                };

                return dialog.ShowDialog(MainWindow) == true ? dialog.FileName : null;
            });
        }

        public string? ShowSaveFileDialog(string title, string filter, string defaultFileName = "")
        {
            return ShowDialogWithTopmost(() =>
            {
                var dialog = new SaveFileDialog
                {
                    Title = title,
                    Filter = filter,
                    FileName = defaultFileName
                };

                return dialog.ShowDialog(MainWindow) == true ? dialog.FileName : null;
            });
        }
    }
}
