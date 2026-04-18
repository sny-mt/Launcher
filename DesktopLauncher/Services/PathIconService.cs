using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLauncher.Interfaces.Services;

namespace DesktopLauncher.Services
{
    public class PathIconService : IPathIconService
    {
        private const int IconSize = 48;
        private const int IconDpi = 96;

        private readonly Lazy<ImageSource> _defaultIcon;
        private readonly Lazy<ImageSource> _urlIcon;

        public PathIconService()
        {
            _defaultIcon = new Lazy<ImageSource>(CreateDefaultIcon);
            _urlIcon = new Lazy<ImageSource>(CreateUrlIcon);
        }

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

                if (UrlHelper.IsUrl(path))
                {
                    return GetUrlIcon();
                }

                var isDirectory = Directory.Exists(path);
                if (!isDirectory && !File.Exists(path))
                {
                    return GetDefaultIcon();
                }

                if (isDirectory)
                {
                    return GetFolderIcon();
                }

                try
                {
                    using var icon = Icon.ExtractAssociatedIcon(path);
                    if (icon != null)
                    {
                        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource.Freeze();
                        return bitmapSource;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ExtractAssociatedIcon failed for '{path}': {ex.Message}");
                }

                var iconFromShell = GetIconFromShell(path);
                if (iconFromShell != null)
                {
                    return iconFromShell;
                }

                var extension = Path.GetExtension(path);
                if (!string.IsNullOrEmpty(extension))
                {
                    var iconFromExtension = GetIconFromExtension(extension);
                    if (iconFromExtension != null)
                    {
                        return iconFromExtension;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetIconFromPath failed for '{path}': {ex.Message}");
            }

            return GetDefaultIcon();
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

                if (extension == ".ico")
                {
                    using var icon = new Icon(iconPath);
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    return bitmapSource;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadCustomIcon failed for '{iconPath}': {ex.Message}");
                return null;
            }
        }

        public ImageSource GetDefaultIcon() => _defaultIcon.Value;

        private static ImageSource CreateDefaultIcon()
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));
                brush.Freeze();
                context.DrawRectangle(brush, null, new Rect(0, 0, IconSize, IconSize));
            }

            var bitmap = new RenderTargetBitmap(IconSize, IconSize, IconDpi, IconDpi, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        private ImageSource? GetIconFromShell(string path)
        {
            var shfi = new SHFILEINFO();
            var result = SHGetFileInfo(path, 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_LARGEICON);

            if (result != IntPtr.Zero && shfi.hIcon != IntPtr.Zero)
            {
                try
                {
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        shfi.hIcon,
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
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        shfi.hIcon,
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

        private ImageSource GetFolderIcon()
        {
            try
            {
                var shellPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var shell32Path = Path.Combine(shellPath, "System32", "shell32.dll");

                if (File.Exists(shell32Path))
                {
                    using var icon = Icon.ExtractAssociatedIcon(shell32Path);
                    if (icon != null)
                    {
                        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource.Freeze();
                        return bitmapSource;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetFolderIcon failed: {ex.Message}");
            }

            return GetDefaultIcon();
        }

        private ImageSource GetUrlIcon() => _urlIcon.Value;

        private static ImageSource CreateUrlIcon()
        {
            var center = IconSize / 2.0;
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var accentColor = System.Windows.Media.Color.FromRgb(0, 120, 215);
                var brush = new SolidColorBrush(accentColor);
                brush.Freeze();
                var pen = new System.Windows.Media.Pen(brush, 2);
                pen.Freeze();

                context.DrawEllipse(null, pen, new System.Windows.Point(center, center), 18, 18);
                context.DrawLine(pen, new System.Windows.Point(6, center), new System.Windows.Point(42, center));
                context.DrawEllipse(null, pen, new System.Windows.Point(center, center), 8, 18);
            }

            var bitmap = new RenderTargetBitmap(IconSize, IconSize, IconDpi, IconDpi, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }
    }
}
