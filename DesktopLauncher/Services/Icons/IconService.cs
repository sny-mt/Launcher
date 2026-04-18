using System.Windows.Media;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;

namespace DesktopLauncher.Services.Icons
{
    /// <summary>
    /// アイコン取得サービスの実装
    /// </summary>
    public class IconService : IIconService
    {
        private readonly IPathIconService _pathIconService;
        private readonly IFaviconService _faviconService;
        private readonly IImageCodecService _imageCodecService;

        public IconService(
            IPathIconService pathIconService,
            IFaviconService faviconService,
            IImageCodecService imageCodecService)
        {
            _pathIconService = pathIconService;
            _faviconService = faviconService;
            _imageCodecService = imageCodecService;
        }

        public ImageSource? GetIconFromPath(string path)
        {
            return _pathIconService.GetIconFromPath(path);
        }

        public ImageSource? LoadCustomIcon(string iconPath)
        {
            return _pathIconService.LoadCustomIcon(iconPath);
        }

        public ImageSource GetDefaultIcon()
        {
            return _pathIconService.GetDefaultIcon();
        }

        public string? DownloadFavicon(string url)
        {
            return _faviconService.DownloadFavicon(url);
        }

        public string? GetIconBase64FromPath(string path)
        {
            var icon = _pathIconService.GetIconFromPath(path);
            return icon != null ? _imageCodecService.ConvertToBase64(icon) : null;
        }

        public string? DownloadFaviconAsBase64(string url)
        {
            return _faviconService.DownloadFaviconAsBase64(url);
        }

        public ImageSource? LoadFromBase64(string base64)
        {
            return _imageCodecService.LoadFromBase64(base64);
        }

        public string? ConvertToBase64(ImageSource imageSource, int size = 48)
        {
            return _imageCodecService.ConvertToBase64(imageSource, size);
        }

    }
}
