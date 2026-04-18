using System;
using System.Diagnostics;
using Microsoft.Win32;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;

namespace DesktopLauncher.Services.Shell
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

            if (string.IsNullOrEmpty(exePath))
            {
                Debug.WriteLine("SetEnabled: exePath is null or empty, skipping registry write.");
                return;
            }

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
            catch (Exception ex)
            {
                Debug.WriteLine($"SetEnabled failed: {ex.Message}");
            }
        }
    }
}
