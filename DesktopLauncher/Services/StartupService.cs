using System;
using Microsoft.Win32;
using DesktopLauncher.Interfaces.Services;

namespace DesktopLauncher.Services
{
    /// <summary>
    /// スタートアップ登録サービスの実装
    /// </summary>
    public class StartupService : IStartupService
    {
        private const string AppName = "DesktopLauncher";
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <inheritdoc/>
        public bool IsEnabled
        {
            get
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
                    return key?.GetValue(AppName) != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public void SetEnabled(bool enable)
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
                if (key == null) return;

                if (enable)
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch
            {
                // レジストリ操作に失敗しても無視
            }
        }
    }
}
