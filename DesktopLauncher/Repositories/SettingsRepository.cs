using DesktopLauncher.Interfaces.Repositories;
using DesktopLauncher.Models;

namespace DesktopLauncher.Repositories
{
    /// <summary>
    /// 設定リポジトリの実装
    /// </summary>
    public class SettingsRepository : ISettingsRepository
    {
        private readonly JsonDataStore _dataStore;

        public SettingsRepository(JsonDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public AppSettings Get()
        {
            return _dataStore.Data.Settings;
        }

        public void Save(AppSettings settings)
        {
            _dataStore.Data.Settings = settings;
            _dataStore.Save();
        }
    }
}
