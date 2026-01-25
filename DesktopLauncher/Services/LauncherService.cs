using System;
using System.Diagnostics;
using System.IO;
using DesktopLauncher.Interfaces.Services;
using DesktopLauncher.Models;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// アイテム起動サービスの実装
    /// </summary>
    public class LauncherService : ILauncherService
    {
        public bool Launch(LauncherItem item, bool runAsAdmin = false)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = item.Path,
                    UseShellExecute = true
                };

                // 引数の設定
                if (!string.IsNullOrEmpty(item.Arguments))
                {
                    startInfo.Arguments = item.Arguments;
                }

                // 作業ディレクトリの設定（URLの場合は不要）
                if (item.ItemType != ItemType.Url)
                {
                    if (!string.IsNullOrEmpty(item.WorkingDirectory) && Directory.Exists(item.WorkingDirectory))
                    {
                        startInfo.WorkingDirectory = item.WorkingDirectory;
                    }
                    else if (item.ItemType == ItemType.Application && File.Exists(item.Path))
                    {
                        // アプリケーションの場合、デフォルトで実行ファイルのディレクトリを作業ディレクトリにする
                        startInfo.WorkingDirectory = Path.GetDirectoryName(item.Path);
                    }
                }

                // 管理者として実行
                if (runAsAdmin)
                {
                    startInfo.Verb = "runas";
                }

                Process.Start(startInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool OpenFileLocation(LauncherItem item)
        {
            try
            {
                if (string.IsNullOrEmpty(item.Path))
                {
                    return false;
                }

                // URLの場合はファイルの場所を開けない
                if (item.ItemType == ItemType.Url)
                {
                    return false;
                }

                string? folderPath;

                if (item.ItemType == ItemType.Folder)
                {
                    folderPath = item.Path;
                }
                else
                {
                    folderPath = Path.GetDirectoryName(item.Path);
                }

                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                {
                    return false;
                }

                // フォルダを開いてファイルを選択状態にする
                if (item.ItemType != ItemType.Folder && File.Exists(item.Path))
                {
                    Process.Start("explorer.exe", $"/select,\"{item.Path}\"");
                }
                else
                {
                    Process.Start("explorer.exe", folderPath);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
