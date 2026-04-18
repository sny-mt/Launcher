using System;
using System.IO;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models.Enums;

namespace DesktopLauncher.Services.Operations
{
    /// <summary>
    /// アイテムタイプ検出サービスの実装
    /// </summary>
    public class ItemTypeDetectionService : IItemTypeDetectionService
    {
        /// <inheritdoc/>
        public (ItemType Type, string Name) DetectFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return (ItemType.File, string.Empty);
            }

            // URLの場合
            if (IsUrl(path))
            {
                var name = ExtractUrlName(path);
                return (ItemType.Url, name);
            }

            // フォルダの場合
            if (Directory.Exists(path))
            {
                var name = new DirectoryInfo(path).Name;
                return (ItemType.Folder, name);
            }

            // 実行ファイルの場合
            var extension = Path.GetExtension(path);
            if (extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                return (ItemType.Application, name);
            }

            // その他のファイル
            var fileName = Path.GetFileName(path);
            return (ItemType.File, fileName);
        }

        /// <inheritdoc/>
        public bool IsUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// URLからホスト名を抽出する
        /// </summary>
        private string ExtractUrlName(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return url;
            }
        }
    }
}
