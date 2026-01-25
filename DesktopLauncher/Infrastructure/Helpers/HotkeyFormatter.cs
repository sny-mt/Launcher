using System.Collections.Generic;
using System.Windows.Input;

namespace DesktopLauncher.Infrastructure.Helpers
{
    /// <summary>
    /// ホットキー表示文字列を生成するヘルパークラス
    /// </summary>
    public static class HotkeyFormatter
    {
        /// <summary>
        /// 修飾キーとキーから表示用文字列を生成する
        /// </summary>
        /// <param name="modifiers">修飾キー</param>
        /// <param name="key">キー</param>
        /// <returns>フォーマットされたホットキー文字列（例: "Ctrl + Alt + Space"）</returns>
        public static string Format(ModifierKeys modifiers, Key key)
        {
            var parts = new List<string>();

            if (modifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");

            parts.Add(key.ToString());

            return string.Join(" + ", parts);
        }

        /// <summary>
        /// キーが修飾キーかどうかを判定する
        /// </summary>
        /// <param name="key">判定するキー</param>
        /// <returns>修飾キーの場合はtrue</returns>
        public static bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LWin || key == Key.RWin ||
                   key == Key.System;
        }
    }
}
