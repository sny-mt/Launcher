using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLauncher.Interfaces.Services;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// アイコン取得サービスの実装
    /// </summary>
    public class IconService : IIconService
    {
        private ImageSource? _defaultIcon;

        // Shell32 API定義
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public ImageSource? GetIconFromPath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return GetDefaultIcon();
                }

                // URLの場合
                if (IsUrl(path))
                {
                    return GetUrlIcon();
                }

                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    return GetDefaultIcon();
                }

                // フォルダの場合
                if (Directory.Exists(path))
                {
                    return GetFolderIcon();
                }

                // 方法1: Icon.ExtractAssociatedIconを試行
                try
                {
                    using var icon = Icon.ExtractAssociatedIcon(path);
                    if (icon != null)
                    {
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch
                {
                    // 失敗した場合は次の方法を試行
                }

                // 方法2: SHGetFileInfoを使用（より信頼性が高い）
                var iconFromShell = GetIconFromShell(path);
                if (iconFromShell != null)
                {
                    return iconFromShell;
                }

                // 方法3: 拡張子からアイコンを取得
                var extension = System.IO.Path.GetExtension(path);
                if (!string.IsNullOrEmpty(extension))
                {
                    var iconFromExtension = GetIconFromExtension(extension);
                    if (iconFromExtension != null)
                    {
                        return iconFromExtension;
                    }
                }
            }
            catch
            {
                // アイコン取得失敗時はデフォルトを返す
            }

            return GetDefaultIcon();
        }

        private ImageSource? GetIconFromShell(string path)
        {
            var shfi = new SHFILEINFO();
            var result = SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_LARGEICON);

            if (result != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
            {
                try
                {
                    using var icon = Icon.FromHandle(shfi.hIcon);
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    return bitmapSource;
                }
                finally
                {
                    DestroyIcon(shfi.hIcon);
                }
            }

            return null;
        }

        private ImageSource? GetIconFromExtension(string extension)
        {
            var shfi = new SHFILEINFO();
            var result = SHGetFileInfo(extension, FILE_ATTRIBUTE_NORMAL, ref shfi, (uint)Marshal.SizeOf(shfi),
                SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES);

            if (result != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
            {
                try
                {
                    using var icon = Icon.FromHandle(shfi.hIcon);
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    return bitmapSource;
                }
                finally
                {
                    DestroyIcon(shfi.hIcon);
                }
            }

            return null;
        }

        public ImageSource? LoadCustomIcon(string iconPath)
        {
            try
            {
                if (string.IsNullOrEmpty(iconPath) || !File.Exists(iconPath))
                {
                    return null;
                }

                var extension = Path.GetExtension(iconPath).ToLowerInvariant();

                // .icoファイルの場合
                if (extension == ".ico")
                {
                    using var icon = new Icon(iconPath);
                    return Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }

                // その他の画像ファイル（png, jpg など）
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public ImageSource GetDefaultIcon()
        {
            if (_defaultIcon != null)
            {
                return _defaultIcon;
            }

            // デフォルトアイコンを生成（シンプルな四角形）
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(
                    new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)),
                    null,
                    new Rect(0, 0, 48, 48));
            }

            var bitmap = new RenderTargetBitmap(48, 48, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            _defaultIcon = bitmap;
            return _defaultIcon;
        }

        private ImageSource GetFolderIcon()
        {
            try
            {
                // システムのフォルダアイコンを取得
                var shellPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var shell32Path = Path.Combine(shellPath, "System32", "shell32.dll");

                if (File.Exists(shell32Path))
                {
                    using var icon = Icon.ExtractAssociatedIcon(shell32Path);
                    if (icon != null)
                    {
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
            }
            catch
            {
                // 失敗時はデフォルトを返す
            }

            return GetDefaultIcon();
        }

        private ImageSource GetUrlIcon()
        {
            // 地球アイコンを描画
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var accentColor = System.Windows.Media.Color.FromRgb(0, 120, 215);
                var brush = new SolidColorBrush(accentColor);
                var pen = new System.Windows.Media.Pen(brush, 2);

                // 外円
                context.DrawEllipse(null, pen, new System.Windows.Point(24, 24), 18, 18);

                // 横線
                context.DrawLine(pen, new System.Windows.Point(6, 24), new System.Windows.Point(42, 24));

                // 縦楕円
                context.DrawEllipse(null, pen, new System.Windows.Point(24, 24), 8, 18);
            }

            var bitmap = new RenderTargetBitmap(48, 48, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            return bitmap;
        }

        public static bool IsUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public string? DownloadFavicon(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !IsUrl(url))
                {
                    return null;
                }

                // URLからドメインを抽出
                var uri = new Uri(url);
                var domain = uri.Host;

                // アイコン保存先ディレクトリを作成
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DesktopLauncher",
                    "Icons");
                Directory.CreateDirectory(appDataPath);

                // ファイル名を生成（ドメイン名ベース）
                var safeFileName = string.Join("_", domain.Split(Path.GetInvalidFileNameChars()));
                var iconPath = Path.Combine(appDataPath, $"{safeFileName}.png");

                // 既にダウンロード済みの場合はそのパスを返す
                if (File.Exists(iconPath))
                {
                    return iconPath;
                }

                // Google Favicon APIを使用してファビコンを取得
                var faviconUrl = $"https://www.google.com/s2/favicons?domain={domain}&sz=64";

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = httpClient.GetAsync(faviconUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var iconData = response.Content.ReadAsByteArrayAsync().Result;

                    // 取得したアイコンが有効かチェック（最小サイズ）
                    if (iconData.Length > 100)
                    {
                        File.WriteAllBytes(iconPath, iconData);
                        return iconPath;
                    }
                }
            }
            catch
            {
                // ダウンロード失敗時はnullを返す
            }

            return null;
        }

        public string? GetIconBase64FromPath(string path)
        {
            try
            {
                var icon = GetIconFromPath(path);
                if (icon != null)
                {
                    return ConvertToBase64(icon);
                }
            }
            catch
            {
                // 失敗時はnullを返す
            }

            return null;
        }

        public string? DownloadFaviconAsBase64(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !IsUrl(url))
                {
                    return null;
                }

                // URLからドメインを抽出
                var uri = new Uri(url);
                var domain = uri.Host;

                // Google Favicon APIを使用してファビコンを取得
                var faviconUrl = $"https://www.google.com/s2/favicons?domain={domain}&sz=64";

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var response = httpClient.GetAsync(faviconUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    var iconData = response.Content.ReadAsByteArrayAsync().Result;

                    // 取得したアイコンが有効かチェック（最小サイズ）
                    if (iconData.Length > 100)
                    {
                        // 画像をリサイズしてBase64に変換
                        using var ms = new MemoryStream(iconData);
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        return ConvertToBase64(bitmap);
                    }
                }
            }
            catch
            {
                // ダウンロード失敗時はnullを返す
            }

            return null;
        }

        public ImageSource? LoadFromBase64(string base64)
        {
            try
            {
                if (string.IsNullOrEmpty(base64))
                {
                    return null;
                }

                var imageData = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(imageData);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public string? ConvertToBase64(ImageSource imageSource, int size = 48)
        {
            try
            {
                if (imageSource == null)
                {
                    return null;
                }

                // BitmapSourceに変換
                BitmapSource bitmapSource;
                if (imageSource is BitmapSource bs)
                {
                    bitmapSource = bs;
                }
                else
                {
                    return null;
                }

                // 指定サイズにリサイズ
                var resized = ResizeBitmap(bitmapSource, size, size);

                // PNGにエンコード
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resized));

                using var ms = new MemoryStream();
                encoder.Save(ms);

                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return null;
            }
        }

        private static BitmapSource ResizeBitmap(BitmapSource source, int width, int height)
        {
            var scaleX = width / source.Width;
            var scaleY = height / source.Height;

            var transform = new ScaleTransform(scaleX, scaleY);
            var resized = new TransformedBitmap(source, transform);

            // RenderTargetBitmapを使って固定サイズに変換
            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawImage(resized, new Rect(0, 0, width, height));
            }
            renderTarget.Render(visual);
            renderTarget.Freeze();

            return renderTarget;
        }
    }
}
