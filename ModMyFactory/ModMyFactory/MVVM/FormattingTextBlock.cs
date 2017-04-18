using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using ModMyFactory.Helpers;

namespace ModMyFactory.MVVM
{
    class FormattingTextBlock : Control
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(FormattingTextBlock), new PropertyMetadata(null, OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            FormattingTextBlock textBlock = source as FormattingTextBlock;
            textBlock?.OnTextChanged(e);
        }


        public event DependencyPropertyChangedEventHandler TextChanged;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected virtual void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);

            ApplyTextFormatted();
        }


        #region Formatting

        private object anchor;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            anchor = Template?.FindName("PART_ContentHost", this);
            ApplyTextFormatted();
        }

        private void ApplyTextFormatted()
        {
            TextBlock textBlock = anchor as TextBlock;
            if (textBlock != null)
            {
                textBlock.Inlines.Clear();

                if (!string.IsNullOrEmpty(Text))
                {
                    AddTextToTextBlockFormatted(textBlock, string.Join(" ", Text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                }
            }
        }

        private static void AddTextToTextBlockFormatted(TextBlock textBlock, string text)
        {
            int index = 0;
            foreach (Inline element in FormatText(text, ref index))
            {
                textBlock.Inlines.Add(element);
            }
        }

        private static bool PatternReached(string text, int index, string[] patterns, out int patternLength)
        {
            patternLength = -1;
            if ((patterns == null) || (patterns.Length == 0)) return false;

            foreach (string pattern in patterns)
            {
                if (text.PositionEquals(index, pattern))
                {
                    patternLength = pattern.Length;
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<Inline> FormatText(string text, ref int index, params string[] endPatterns)
        {
            List<Inline> result = new List<Inline>();
            
            int startIndex = index;
            while (index < text.Length)
            {
                int patternLength;
                if (PatternReached(text, index, endPatterns, out patternLength))
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));

                    index += patternLength;
                    return result;
                }

                char c = text[index];
                if (c == '\\') // Escape char
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));

                    index += 2;
                    startIndex = index - 1;
                }
                else if (text.PositionEquals(index, "####")) // Heading level 4
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                    index += 4;

                    var elements = FormatText(text, ref index, "####", "###", "##", "#", "\r\n", "\r", "\n");
                    var span = new Span() { TextDecorations = new TextDecorationCollection(TextDecorations.Underline) };
                    foreach (Inline element in elements)
                        span.Inlines.Add(element);
                    result.Add(span);
                    result.Add(new LineBreak());

                    startIndex = index;
                }
                else if (text.PositionEquals(index, "###")) // Heading level 3
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                    index += 3;

                    var elements = FormatText(text, ref index, "###", "##", "#", "\r\n", "\r", "\n");
                    var span = new Span() { FontWeight = FontWeights.Bold };
                    foreach (Inline element in elements)
                        span.Inlines.Add(element);
                    result.Add(span);
                    result.Add(new LineBreak());

                    startIndex = index;
                }
                else if (text.PositionEquals(index, "##")) // Sub-heading
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                    index += 2;

                    var elements = FormatText(text, ref index, "##", "#", "\r\n", "\r", "\n");
                    var span = new Span() { FontSize = 26 };
                    foreach (Inline element in elements)
                        span.Inlines.Add(element);
                    result.Add(span);
                    result.Add(new LineBreak());

                    startIndex = index;
                }
                else if (c == '#') // Heading
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                    index++;

                    var elements = FormatText(text, ref index, "#", "\r\n", "\r", "\n");
                    var span = new Span() { FontSize = 20 };
                    foreach (Inline element in elements)
                        span.Inlines.Add(element);
                    result.Add(span);
                    result.Add(new LineBreak());

                    startIndex = index;
                }
                else if (text.PositionEquals(index, "\r\n")) // New line
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                    result.Add(new LineBreak());

                    index += 2;
                    startIndex = index;
                }
                else if ((c == '\r') || (c == '\n')) // New line
                {
                    result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                    result.Add(new LineBreak());

                    index++;
                    startIndex = index;
                }
                else if (text.PositionEquals(index, "**")) // Bold
                {
                    if ((index == startIndex) || char.IsWhiteSpace(text[index - 1]))
                    {
                        result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                        index += 2;

                        var elements = FormatText(text, ref index, "**");
                        var span = new Span() { FontWeight = FontWeights.Bold };
                        foreach (Inline element in elements)
                            span.Inlines.Add(element);
                        result.Add(span);

                        startIndex = index;
                    }
                    else
                    {
                        index += 2;
                    }
                }
                else if (c == '_') // Italic
                {
                    if ((index == startIndex) || char.IsWhiteSpace(text[index - 1]))
                    {
                        result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                        index++;

                        var elements = FormatText(text, ref index, "_");
                        var span = new Span() { FontStyle = FontStyles.Italic };
                        foreach (Inline element in elements)
                            span.Inlines.Add(element);
                        result.Add(span);

                        startIndex = index;
                    }
                    else
                    {
                        index++;
                    }
                }
                else if (c == '[') // Link
                {
                    int endIndex = text.IndexOf(']', index);
                    if (endIndex > -1)
                    {
                        result.Add(new Run(text.Substring(startIndex, index - startIndex)));
                        index++;

                        var link = new Hyperlink(new Run(text.Substring(index, endIndex - index)));
                        link.RequestNavigate += LinkOnRequestNavigate;

                        index = endIndex + 1;

                        if (text[index] == '(')
                        {
                            endIndex = text.IndexOf(')', index);
                            if (endIndex > -1)
                            {
                                index++;

                                string url = text.Substring(index, endIndex - index).SplitOnWhitespace()[0];
                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(url))
                                    {
                                        var uri = new Uri(url);
                                        link.NavigateUri = uri;
                                    }
                                }
                                catch (UriFormatException)
                                { }

                                index = endIndex + 1;
                            }
                        }
                        result.Add(link);

                        startIndex = index;
                    }
                }
                else if (text.PositionEquals(index, "http://") || text.PositionEquals(index, "https://")) // Implicit link
                {
                    if ((index == startIndex) || char.IsWhiteSpace(text[index - 1]))
                    {
                        result.Add(new Run(text.Substring(startIndex, index - startIndex)));

                        int endIndex = MathHelper.Min(text.IndexOf(' ', index), text.IndexOf("\r\n", index, StringComparison.Ordinal), text.IndexOf('\r', index), text.IndexOf('\n', index), text.Length - 1);
                        string url = text.Substring(index, endIndex - index);

                        var link = new Hyperlink(new Run(url));
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(url))
                            {
                                var uri = new Uri(url);
                                link.NavigateUri = uri;
                            }
                        }
                        catch (UriFormatException)
                        { }
                        link.RequestNavigate += LinkOnRequestNavigate;
                        result.Add(link);

                        index = endIndex;
                        startIndex = index;
                    }
                    else
                    {
                        index++;
                    }
                }
                else
                {
                    index++;
                }
            }

            int length = Math.Min(index - startIndex, text.Length - startIndex);
            if (length > 0) result.Add(new Run(text.Substring(startIndex, length)));

            return result;
        }

        private static void LinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string url = e.Uri?.ToString();
                if (!string.IsNullOrWhiteSpace(url)) Process.Start(url);
            }
            catch { }
        }

        #endregion
    }
}
