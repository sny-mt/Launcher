using System;
using System.IO;
using DesktopLauncher.Models;
using Newtonsoft.Json;

namespace DesktopLauncher.Repositories
{
    /// <summary>
    /// JSONデータストア（データの読み書きを一元管理）
    /// </summary>
    public class JsonDataStore
    {
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
                File.WriteAllText(_filePath, json);
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
                    var json = File.ReadAllText(_filePath);
                    var data = JsonConvert.DeserializeObject<LauncherData>(json);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch
            {
                // 読み込み失敗時は新規データを作成
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
