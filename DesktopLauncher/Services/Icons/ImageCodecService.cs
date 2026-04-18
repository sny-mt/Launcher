using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;

namespace DesktopLauncher.Services.Icons
{
    public class ImageCodecService : IImageCodecService
    {
        private const int DefaultDpi = 96;

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
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadFromBase64 failed: {ex.Message}");
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

                if (imageSource is not BitmapSource bitmapSource)
                {
                    return null;
                }

                var resized = ResizeBitmap(bitmapSource, size, size);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resized));

                using var ms = new MemoryStream();
                encoder.Save(ms);

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConvertToBase64 failed: {ex.Message}");
                return null;
            }
        }

        private static BitmapSource ResizeBitmap(BitmapSource source, int width, int height)
        {
            if (source.PixelWidth == 0 || source.PixelHeight == 0) return source;

            var scaleX = width / source.Width;
            var scaleY = height / source.Height;

            var transform = new ScaleTransform(scaleX, scaleY);
            var resized = new TransformedBitmap(source, transform);

            var renderTarget = new RenderTargetBitmap(width, height, DefaultDpi, DefaultDpi, PixelFormats.Pbgra32);
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
