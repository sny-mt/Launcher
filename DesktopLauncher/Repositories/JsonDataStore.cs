using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DesktopLauncher.Models;
using Newtonsoft.Json;

namespace DesktopLauncher.Repositories
{
    /// <summary>
    /// JSONデータストア（データの読み書きを一元管理）
    /// </summary>
    public class JsonDataStore
    {
        public string FilePath => _filePath;
        private readonly string _filePath;
        private LauncherData _data;
        private readonly object _lock = new object();

        public JsonDataStore()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DesktopLauncher");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _filePath = Path.Combine(appDataPath, "launcher_data.json");
            _data = Load();
        }

        /// <summary>
        /// データを取得
        /// </summary>
        public LauncherData Data
        {
            get
            {
                lock (_lock)
                {
                    return _data;
                }
            }
        }

        /// <summary>
        /// データを保存
        /// </summary>
        public void Save()
        {
            lock (_lock)
            {
                var json = JsonConvert.SerializeObject(_data, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None
                });

                var tempFilePath = $"{_filePath}.tmp";
                var backupFilePath = $"{_filePath}.bak";

                File.WriteAllText(tempFilePath, json, Encoding.UTF8);

                if (File.Exists(_filePath))
                {
                    File.Replace(tempFilePath, _filePath, backupFilePath, true);
                }
                else
                {
                    File.Move(tempFilePath, _filePath);
                }
            }
        }

        /// <summary>
        /// データを再読み込み
        /// </summary>
        public void Reload()
        {
            lock (_lock)
            {
                _data = Load();
            }
        }

        private LauncherData Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath, Encoding.UTF8);
                    var data = JsonConvert.DeserializeObject<LauncherData>(json);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Data load failed from '{_filePath}': {ex.Message}");
            }

            // デフォルトデータを作成
            return CreateDefaultData();
        }

        private LauncherData CreateDefaultData()
        {
            var data = new LauncherData();

            // デフォルトカテゴリを追加
            data.Categories.Add(new Category
            {
                Name = "アプリ",
                SortOrder = 0
            });

            data.Categories.Add(new Category
            {
                Name = "フォルダ",
                SortOrder = 1
            });

            data.Categories.Add(new Category
            {
                Name = "ファイル",
                SortOrder = 2
            });

            return data;
        }
    }
}
