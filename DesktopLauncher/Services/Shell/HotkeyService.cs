using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using DesktopLauncher.Interfaces.Services.Icons;
using DesktopLauncher.Interfaces.Services.Data;
using DesktopLauncher.Interfaces.Services.Operations;
using DesktopLauncher.Interfaces.Services.Ui;
using DesktopLauncher.Interfaces.Services.Shell;
using DesktopLauncher.Interfaces.Services.Search;

namespace DesktopLauncher.Services.Shell
{
    /// <summary>
    /// グローバルホットキーサービスの実装
    /// </summary>
    public class HotkeyService : IHotkeyService
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource? _source;
        private IntPtr _windowHandle;
        private bool _isRegistered;

        public event EventHandler? HotkeyPressed;

        public bool Register(ModifierKeys modifiers, Key key)
        {
            if (_isRegistered)
            {
                Unregister();
            }

            var window = Application.Current.MainWindow;
            if (window == null) return false;

            var helper = new WindowInteropHelper(window);
            _windowHandle = helper.EnsureHandle();
            _source = HwndSource.FromHwnd(_windowHandle);
            if (_source == null) return false;
            _source.AddHook(HwndHook);

            uint mod = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt)) mod |= MOD_ALT;
            if (modifiers.HasFlag(ModifierKeys.Control)) mod |= MOD_CONTROL;
            if (modifiers.HasFlag(ModifierKeys.Shift)) mod |= MOD_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Windows)) mod |= MOD_WIN;

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, mod, vk);
            return _isRegistered;
        }

        public void Unregister()
        {
            if (_isRegistered && _windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }

            _source?.RemoveHook(HwndHook);
            _source = null;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
        }
    }
}
