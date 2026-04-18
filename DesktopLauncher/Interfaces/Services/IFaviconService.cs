namespace DesktopLauncher.Interfaces.Services
{
    public interface IFaviconService
    {
        string? DownloadFavicon(string url);
        string? DownloadFaviconAsBase64(string url);
    }
}
