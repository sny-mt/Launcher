using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DesktopLauncher.Infrastructure.Controls
{
    /// <summary>
    /// 検索キーワードをハイライト表示するTextBlock
    /// </summary>
    public class HighlightTextBlock : TextBlock
    {
        public static readonly DependencyProperty HighlightTextProperty =
            DependencyProperty.Register(
                nameof(HighlightText),
                typeof(string),
                typeof(HighlightTextBlock),
                new PropertyMetadata(string.Empty, OnHighlightChanged));

        public static readonly DependencyProperty SourceTextProperty =
            DependencyProperty.Register(
                nameof(SourceText),
                typeof(string),
                typeof(HighlightTextBlock),
                new PropertyMetadata(string.Empty, OnHighlightChanged));

        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register(
                nameof(HighlightBrush),
                typeof(Brush),
                typeof(HighlightTextBlock),
                new PropertyMetadata(Brushes.Yellow, OnHighlightChanged));

        public static readonly DependencyProperty HighlightForegroundProperty =
            DependencyProperty.Register(
                nameof(HighlightForeground),
                typeof(Brush),
                typeof(HighlightTextBlock),
                new PropertyMetadata(null, OnHighlightChanged));

        public string HighlightText
        {
            get => (string)GetValue(HighlightTextProperty);
            set => SetValue(HighlightTextProperty, value);
        }

        public string SourceText
        {
            get => (string)GetValue(SourceTextProperty);
            set => SetValue(SourceTextProperty, value);
        }

        public Brush HighlightBrush
        {
            get => (Brush)GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        public Brush HighlightForeground
        {
            get => (Brush)GetValue(HighlightForegroundProperty);
            set => SetValue(HighlightForegroundProperty, value);
        }

        private static void OnHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HighlightTextBlock control)
            {
                control.UpdateHighlight();
            }
        }

        private void UpdateHighlight()
        {
            Inlines.Clear();

            var sourceText = SourceText ?? string.Empty;
            var highlightText = HighlightText ?? string.Empty;

            if (string.IsNullOrEmpty(sourceText))
            {
                return;
            }

            if (string.IsNullOrEmpty(highlightText))
            {
                Inlines.Add(new Run(sourceText));
                return;
            }

            var currentIndex = 0;
            var lowerSource = sourceText.ToLower();
            var lowerHighlight = highlightText.ToLower();

            while (currentIndex < sourceText.Length)
            {
                var matchIndex = lowerSource.IndexOf(lowerHighlight, currentIndex, StringComparison.Ordinal);

                if (matchIndex < 0)
                {
                    // 残りのテキストを追加
                    Inlines.Add(new Run(sourceText.Substring(currentIndex)));
                    break;
                }

                // マッチ前のテキストを追加
                if (matchIndex > currentIndex)
                {
                    Inlines.Add(new Run(sourceText.Substring(currentIndex, matchIndex - currentIndex)));
                }

                // ハイライト部分を追加
                var highlightRun = new Run(sourceText.Substring(matchIndex, highlightText.Length))
                {
                    Background = HighlightBrush,
                    FontWeight = FontWeights.Bold
                };

                if (HighlightForeground != null)
                {
                    highlightRun.Foreground = HighlightForeground;
                }

                Inlines.Add(highlightRun);

                currentIndex = matchIndex + highlightText.Length;
            }
        }
    }
}
