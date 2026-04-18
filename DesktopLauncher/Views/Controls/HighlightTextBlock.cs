using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DesktopLauncher.Views.Controls
{
    /// <summary>
    /// 検索キーワードをハイライト表示するTextBlock
    /// スペース区切りで複数キーワードに対応
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

            // スペースで分割して複数キーワードに対応
            var keywords = highlightText
                .Split(new[] { ' ', '\u3000' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (keywords.Length == 0)
            {
                Inlines.Add(new Run(sourceText));
                return;
            }

            // 各文字位置がハイライト対象かどうかをマーク
            var highlightFlags = new bool[sourceText.Length];
            var lowerSource = sourceText.ToLower();

            foreach (var keyword in keywords)
            {
                var lowerKeyword = keyword.ToLower();
                var searchIndex = 0;

                while (searchIndex < lowerSource.Length)
                {
                    var matchIndex = lowerSource.IndexOf(lowerKeyword, searchIndex, StringComparison.Ordinal);
                    if (matchIndex < 0) break;

                    // マッチした範囲をマーク
                    for (int i = matchIndex; i < matchIndex + keyword.Length && i < highlightFlags.Length; i++)
                    {
                        highlightFlags[i] = true;
                    }

                    searchIndex = matchIndex + 1;
                }
            }

            // フラグに基づいてRunを生成
            var currentIndex = 0;
            while (currentIndex < sourceText.Length)
            {
                var isHighlight = highlightFlags[currentIndex];
                var endIndex = currentIndex + 1;

                // 同じ状態が続く範囲を探す
                while (endIndex < sourceText.Length && highlightFlags[endIndex] == isHighlight)
                {
                    endIndex++;
                }

                var text = sourceText.Substring(currentIndex, endIndex - currentIndex);

                if (isHighlight)
                {
                    var highlightRun = new Run(text)
                    {
                        Background = HighlightBrush,
                        FontWeight = FontWeights.Bold
                    };

                    if (HighlightForeground != null)
                    {
                        highlightRun.Foreground = HighlightForeground;
                    }

                    Inlines.Add(highlightRun);
                }
                else
                {
                    Inlines.Add(new Run(text));
                }

                currentIndex = endIndex;
            }
        }
    }
}
