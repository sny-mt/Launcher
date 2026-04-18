namespace DesktopLauncher.Interfaces.Services
{
    public interface IDataExportService
    {
        bool ExportToFile(string filePath);
        bool ImportFromFile(string filePath);
    }
}
