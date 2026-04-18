using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;
using DesktopLauncher.Models;
using DesktopLauncher.Repositories;
using Newtonsoft.Json;

namespace DesktopLauncher.Services.Data
{
    public class DataExportService : IDataExportService
    {
        private readonly JsonDataStore _dataStore;

        public DataExportService(JsonDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public bool ExportToFile(string filePath)
        {
            try
            {
                var sourceFile = _dataStore.FilePath;
                if (!File.Exists(sourceFile))
                {
                    return false;
                }

                File.Copy(sourceFile, filePath, true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Export failed: {ex.Message}");
                return false;
            }
        }

        public bool ImportFromFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var data = JsonConvert.DeserializeObject<LauncherData>(json);

                if (data == null || data.Categories == null || data.Items == null)
                {
                    return false;
                }

                // データストアのファイルを上書きしてリロード
                File.WriteAllText(_dataStore.FilePath, json, Encoding.UTF8);
                _dataStore.Reload();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Import failed: {ex.Message}");
                return false;
            }
        }
    }
}
