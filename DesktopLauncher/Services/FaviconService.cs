using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using DesktopLauncher.Interfaces.Services;

namespace DesktopLauncher.Services
{
    public class FaviconService : IFaviconService
    {
        /// <summary>
        /// ファビコンの最小有効サイズ（バイト）。これ以下はプレースホルダー画像とみなす。
        /// </summary>
        private const int MinValidIconSize = 100;

        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        static FaviconService()
        {
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DesktopLauncher/1.0");
        }

        private readonly IImageCodecService _imageCodecService;

        public FaviconService(IImageCodecService imageCodecService)
        {
            _imageCodecService = imageCodecService;
        }

        public string? DownloadFavicon(string url)
        {
            try
            {
                if (!TryGetDomain(url, out var domain))
                {
                    return null;
                }

                var iconPath = GetCachedIconPath(domain);
                if (File.Exists(iconPath))
                {
                    return iconPath;
                }

                var iconData = DownloadFaviconBytes(domain);
                if (!IsValidIconData(iconData))
                {
                    return null;
                }

                File.WriteAllBytes(iconPath, iconData!);
                return iconPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DownloadFavicon failed for '{url}': {ex.Message}");
                return null;
            }
        }

        public string? DownloadFaviconAsBase64(string url)
        {
            try
            {
                if (!TryGetDomain(url, out var domain))
                {
                    return null;
                }

                // キャッシュ済みファイルがあればそこから読む
                var cachedPath = GetCachedIconPath(domain);
                byte[]? iconData;
                if (File.Exists(cachedPath))
                {
                    iconData = File.ReadAllBytes(cachedPath);
                }
                else
                {
                    iconData = DownloadFaviconBytes(domain);
                    if (!IsValidIconData(iconData))
                    {
                        return null;
                    }

                    File.WriteAllBytes(cachedPath, iconData!);
                }

                using var ms = new MemoryStream(iconData!);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return _imageCodecService.ConvertToBase64(bitmap);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DownloadFaviconAsBase64 failed for '{url}': {ex.Message}");
                return null;
            }
        }

        private static bool TryGetDomain(string url, out string domain)
        {
            domain = string.Empty;

            if (!UrlHelper.TryParseHttpUrl(url, out var uri))
            {
                return false;
            }

            domain = uri!.Host;
            return !string.IsNullOrWhiteSpace(domain);
        }

        private static string GetCachedIconPath(string domain)
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DesktopLauncher",
                "Icons");
            Directory.CreateDirectory(appDataPath);

            var safeFileName = string.Join("_", domain.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(appDataPath, $"{safeFileName}.png");
        }

        private static byte[]? DownloadFaviconBytes(string domain)
        {
            var faviconUrl = $"https://www.google.com/s2/favicons?domain={domain}&sz=64";
            using var response = HttpClient.GetAsync(faviconUrl).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        }

        private static bool IsValidIconData(byte[]? iconData)
        {
            return iconData != null && iconData.Length > MinValidIconSize;
        }
    }
}
