using System.Windows.Media;

namespace DesktopLauncher.Interfaces.Services.Icons
{
    public interface IImageCodecService
    {
        ImageSource? LoadFromBase64(string base64);
        string? ConvertToBase64(ImageSource imageSource, int size = 48);
    }
}
