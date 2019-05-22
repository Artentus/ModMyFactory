using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using ModMyFactory.Helpers;

namespace ModMyFactory.Controls
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

        private bool LineIsSeparator(string line)
        {
            bool containsMinus = false;
            bool containsOnlyMinusAndWhitespace = true;

            foreach (char c in line)
            {
                if ((c == '-') || (c == '*')) containsMinus = true;
                else if (!char.IsWhiteSpace(c)) containsOnlyMinusAndWhitespace = false;
            }

            return containsMinus && containsOnlyMinusAndWhitespace;
        }

        private string RemoveSeparatorLines(string value)
        {
            var sb = new StringBuilder(value.Length);

            string[] lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (!LineIsSeparator(line))
                    sb.AppendLine(line);
                else
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private void ApplyTextFormatted()
        {
            TextBlock textBlock = anchor as TextBlock;
            if (textBlock != null)
            {
                textBlock.Inlines.Clear();

                if (!string.IsNullOrEmpty(Text))
                {
                    AddTextToTextBlockFormatted(textBlock, RemoveSeparatorLines(string.Join(" ", Text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries))));
                }
            }
        }

        private static void AddTextToTextBlockFormatted(TextBlock textBlock, string text)
        {
            int index = 0;
            FormatText(text, textBlock.Inlines, ref index);
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

        private static void FormatText(string text, ICollection<Inline> inlines, ref int index, params string[] endPatterns)
        {
            int length;

            int startIndex = index;
            while (index < text.Length)
            {
                length = index - startIndex;

                int patternLength;
                if (PatternReached(text, index, endPatterns, out patternLength))
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                    index += patternLength;
                    return;
                }

                char c = text[index];
                if (c == '\\') // Escape char
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                    index += 2;
                    startIndex = index - 1;
                }
                else if (text.PositionEquals(index, "####")) // Heading level 4
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                    index += 4;
                    while (char.IsWhiteSpace(text[index])) index++;

                    var span = new Span() { TextDecorations = new TextDecorationCollection(TextDecorations.Underline) };
                    FormatText(text, span.Inlines, ref index, "####", "###", "##", "#", "\r\n", "\r", "\n");
                    inlines.Add(span);
                    inlines.Add(new LineBreak());

                    startIndex = index;
                }
                else if (text.PositionEquals(index, "###")) // Heading level 3
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                    index += 3;
                    while (char.IsWhiteSpace(text[index])) index++;

                    var span = new Span() { FontWeight = FontWeights.Bold };
                    FormatText(text, span.Inlines, ref index, "###", "##", "#", "\r\n", "\r", "\n");
                    inlines.Add(span);
                    inlines.Add(new LineBreak());

                    startIndex = index;
                }
                else if (text.PositionEquals(index, "##")) // Sub-heading
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                    index += 2;
                    while (char.IsWhiteSpace(text[index])) index++;

                    var span = new Span() { FontSize = 18 };
                    FormatText(text, span.Inlines, ref index, "##", "#", "\r\n", "\r", "\n");
                    inlines.Add(span);
                    inlines.Add(new LineBreak());

                    startIndex = index;
                }
                else if (c == '#') // Heading
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                    index++;
                    while (char.IsWhiteSpace(text[index])) index++;

                    var span = new Span() { FontSize = 20 };
                    FormatText(text, span.Inlines, ref index, "#", "\r\n", "\r", "\n");
                    inlines.Add(span);
                    inlines.Add(new LineBreak());

                    startIndex = index;
                }
                else if (text.PositionEquals(index, "\r\n")) // New line
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));
                    inlines.Add(new LineBreak());

                    index += 2;
                    startIndex = index;
                }
                else if ((c == '\r') || (c == '\n')) // New line
                {
                    if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));
                    inlines.Add(new LineBreak());

                    index++;
                    startIndex = index;
                }
                else if (text.PositionEquals(index, "**")) // Bold
                {
                    if ((index == startIndex) || char.IsWhiteSpace(text[index - 1]))
                    {
                        if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                        index += 2;

                        var span = new Span() { FontWeight = FontWeights.Bold };
                        FormatText(text, span.Inlines, ref index, "**");
                        inlines.Add(span);

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
                        if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                        index++;

                        var span = new Span() { FontStyle = FontStyles.Italic };
                        FormatText(text, span.Inlines, ref index, "_");
                        inlines.Add(span);

                        startIndex = index;
                    }
                    else
                    {
                        index++;
                    }
                }
                else if ((c == '*') || (c == '-')) // List
                {
                    if (((index == startIndex) || string.IsNullOrWhiteSpace(text.Substring(startIndex, length))
                        && ((inlines.Count == 0) || (inlines.Last() is LineBreak)))
                        && ((index + 1 >= text.Length) || (text[index + 1] != '-')))
                    {
                        index++;
                        if (text[index] == ' ') index++;

                        var list = new InlineList();
                        list.ApplyTemplate();
                        var container = new InlineUIContainer(list);
                        FormatText(text, list.Inlines, ref index, "\r\n", "\r", "\n");
                        inlines.Add(container);
                        inlines.Add(new LineBreak());

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
                        if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

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

                                int diff = endIndex - index;
                                if (diff > 0)
                                {
                                    string url = text.Substring(index, diff).SplitOnWhitespace()[0];
                                    if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
                                        link.NavigateUri = uri;
                                }

                                index = endIndex + 1;
                            }
                        }
                        inlines.Add(link);

                        startIndex = index;
                    }
                    else
                    {
                        index++;
                    }
                }
                else if (text.PositionEquals(index, "http://") || text.PositionEquals(index, "https://")) // Implicit link
                {
                    if ((index == startIndex) || char.IsWhiteSpace(text[index - 1]))
                    {
                        if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));

                        int[] endings =
                        {
                            text.IndexOf(' ', index), text.IndexOf("\r\n", index, StringComparison.Ordinal), text.IndexOf('\r', index), text.IndexOf('\n', index), text.Length
                        };
                        int endIndex = endings.Where((i) => i > -1).Min();
                        string url = text.Substring(index, endIndex - index);

                        var link = new Hyperlink(new Run(url));
                        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
                            link.NavigateUri = uri;
                        link.RequestNavigate += LinkOnRequestNavigate;
                        inlines.Add(link);

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

            length = Math.Min(index - startIndex, text.Length - startIndex);
            if (length > 0) inlines.Add(new Run(text.Substring(startIndex, length)));
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
