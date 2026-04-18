using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopLauncher.Infrastructure.Helpers
{
    /// <summary>
    /// 画面上の色をマウスで取得するスポイトツール
    /// </summary>
    public class ScreenColorPicker
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int x, int y);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// マウスカーソル位置の画面上の色を取得する
        /// </summary>
        public static Color GetColorAtCursor()
        {
            GetCursorPos(out var point);
            var hdc = GetDC(IntPtr.Zero);
            try
            {
                var pixel = GetPixel(hdc, point.X, point.Y);
                var r = (byte)(pixel & 0xFF);
                var g = (byte)((pixel >> 8) & 0xFF);
                var b = (byte)((pixel >> 16) & 0xFF);
                return Color.FromRgb(r, g, b);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        /// <summary>
        /// 色をHEX文字列に変換する
        /// </summary>
        public static string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// スポイトモードの全画面オーバーレイを表示し、クリックで色を取得する
        /// </summary>
        public static string? PickColorFromScreen(Window owner)
        {
            var overlay = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                Topmost = true,
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                Cursor = Cursors.Cross,
                ShowInTaskbar = false
            };

            // プレビュー用のBorder
            var preview = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false
            };

            var hexLabel = new TextBlock
            {
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false,
                Padding = new Thickness(4, 2, 4, 2),
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0))
            };

            var canvas = new Canvas();
            canvas.Children.Add(preview);
            canvas.Children.Add(hexLabel);
            overlay.Content = canvas;

            string? result = null;

            overlay.MouseMove += (s, e) =>
            {
                var color = GetColorAtCursor();
                preview.Background = new SolidColorBrush(color);
                hexLabel.Text = ColorToHex(color);

                var pos = e.GetPosition(overlay);
                Canvas.SetLeft(preview, pos.X + 15);
                Canvas.SetTop(preview, pos.Y + 15);
                Canvas.SetLeft(hexLabel, pos.X + 15);
                Canvas.SetTop(hexLabel, pos.Y + 58);
            };

            overlay.MouseLeftButtonDown += (s, e) =>
            {
                var color = GetColorAtCursor();
                result = ColorToHex(color);
                overlay.Close();
            };

            overlay.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    overlay.Close();
                }
            };

            overlay.ShowDialog();
            return result;
        }
    }
}
