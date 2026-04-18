namespace DesktopLauncher.Interfaces.Services.Icons
{
    public interface IFaviconService
    {
        string? DownloadFavicon(string url);
        string? DownloadFaviconAsBase64(string url);
    }
}
