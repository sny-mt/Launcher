namespace DesktopLauncher.Interfaces.Services.Data
{
    public interface IDataExportService
    {
        bool ExportToFile(string filePath);
        bool ImportFromFile(string filePath);
    }
}
