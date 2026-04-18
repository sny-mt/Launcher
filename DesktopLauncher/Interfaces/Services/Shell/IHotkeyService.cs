using System;
using System.Windows.Input;

namespace DesktopLauncher.Interfaces.Services.Shell
{
    /// <summary>
    /// グローバルホットキーサービスのインターフェース
    /// </summary>
    public interface IHotkeyService : IDisposable
    {
        /// <summary>
        /// ホットキーが押されたときに発火するイベント
        /// </summary>
        event EventHandler? HotkeyPressed;

        /// <summary>
        /// ホットキーを登録する
        /// </summary>
        /// <param name="modifiers">修飾キー</param>
        /// <param name="key">キー</param>
        /// <returns>登録成功したかどうか</returns>
        bool Register(ModifierKeys modifiers, Key key);

        /// <summary>
        /// ホットキーの登録を解除する
        /// </summary>
        void Unregister();
    }
}
