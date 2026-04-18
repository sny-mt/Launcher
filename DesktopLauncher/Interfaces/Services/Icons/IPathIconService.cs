using System.Windows.Media;

namespace DesktopLauncher.Interfaces.Services.Icons
{
    public interface IPathIconService
    {
        ImageSource? GetIconFromPath(string path);
        ImageSource? LoadCustomIcon(string iconPath);
        ImageSource GetDefaultIcon();
    }
}
