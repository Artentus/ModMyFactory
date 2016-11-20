using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ModMyFactory.Helpers
{
    static class StringHelper
    {
        private class StringPair : IEquatable<StringPair>
        {
            readonly int hashCode;

            public string First { get; }

            public string Second { get; }

            public CultureInfo Culture { get; }

            public CompareOptions Options { get; }

            public StringPair(string first, string second, CultureInfo culture, CompareOptions options)
            {
                First = first;
                Second = second;
                Culture = culture;
                Options = options;

                hashCode = first.GetHashCode() ^ second.GetHashCode() ^ culture.GetHashCode() ^ options.GetHashCode();
            }

            public bool Equals(StringPair other)
            {
                if (other == null) return false;

                return (this.First == other.First) && (this.Second == other.Second)
                    && (this.Culture.Equals(other.Culture)) && (this.Options == other.Options);
            }

            public override bool Equals(object obj) => Equals(obj as StringPair);

            public override int GetHashCode() => hashCode;
        }


        private static int[,] CreateTable(int size)
        {
            int[,] table = new int[size, size];
            for (int i = 0; i < size; i++)
            {
                table[i, 0] = i;
                table[0, i] = i;
            }

            return table;
        }

        static int tableSize = -1;
        static int[,] cachedTable;

        private static int[,] GetTable(int minWidth, int minHeight)
        {
            int minSize = Math.Max(minWidth, minHeight);
            if (tableSize < minSize)
            {
                tableSize = minSize * 2;
                cachedTable = CreateTable(tableSize);
            }

            return cachedTable;
        }

        private static int GetTableMinimum(int[,] table, int x, int y)
        {
            int min = Math.Min(table[x - 1, y - 1], table[x, y - 1]);
            min = Math.Min(min, table[x - 1, y]);

            return min;
        }


        private static int CalculateEditDistance(StringPair pair)
        {
            int[,] table = GetTable(pair.First.Length + 1, pair.Second.Length + 1);

            for (int x = 1; x <= pair.First.Length; x++)
            {
                string cx = pair.First[x - 1].ToString();

                for (int y = 1; y <= pair.Second.Length; y++)
                {
                    string cy = pair.Second[y - 1].ToString();

                    if (pair.Culture.CompareInfo.Compare(cx, cy, pair.Options) == 0)
                    {
                        table[x, y] = table[x - 1, y - 1];
                    }
                    else
                    {
                        table[x, y] = GetTableMinimum(table, x, y) + 1;
                    }
                }
            }

            return table[pair.First.Length, pair.Second.Length];
        }

        static readonly Dictionary<StringPair, int> CachedEditDistances = new Dictionary<StringPair, int>();

        /// <summary>
        /// Calculates the edit distance of two strings.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <param name="culture">The culture that is used to compare the strings.</param>
        /// <param name="options">A value that specifies how two string are compared.</param>
        /// <returns>Returns the minimum edit distance between the two input strings.</returns>
        public static int GetEditDistance(string first, string second, CultureInfo culture, CompareOptions options)
        {
            if (string.IsNullOrEmpty(first) || (first.Length == 0)) return second?.Length ?? 0;
            if (string.IsNullOrEmpty(second) || (second.Length == 0)) return first.Length;

            var pair = new StringPair(first, second, culture, options);
            int distance;
            if (!CachedEditDistances.TryGetValue(pair, out distance))
            {
                distance = CalculateEditDistance(pair);
                CachedEditDistances.Add(pair, distance);
            }

            return distance;
        }

        /// <summary>
        /// Calculates the edit distance of two strings.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <param name="culture">The culture that is used to compare the strings.</param>
        /// <returns>Returns the minimum edit distance between the two input strings.</returns>
        public static int GetEditDistance(string first, string second, CultureInfo culture)
        {
            return GetEditDistance(first, second, culture, CompareOptions.None);
        }

        /// <summary>
        /// Calculates the edit distance of two strings.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <returns>Returns the minimum edit distance between the two input strings.</returns>
        public static int GetEditDistance(string first, string second)
        {
            return GetEditDistance(first, second, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        private static bool FilterWordIsContained(string filterWord, IEnumerable<string> modWords)
        {
            foreach (string modWord in modWords)
            {
                int lengthDifference = Math.Abs(modWord.Length - filterWord.Length);
                if (modWord.Length < filterWord.Length)
                {
                    if (lengthDifference > Math.Ceiling(modWord.Length * 0.2)) continue;

                    for (int i = 0; i <= lengthDifference; i++)
                    {
                        string subFilterWord = filterWord.Substring(i, modWord.Length);

                        int distance = subFilterWord.EditDistanceTo(modWord, Thread.CurrentThread.CurrentCulture, CompareOptions.IgnoreCase);
                        if (distance <= Math.Ceiling(modWord.Length * 0.2)) return true;
                    }
                }
                else
                {
                    for (int i = 0; i <= lengthDifference; i++)
                    {
                        string subModWord = modWord.Substring(i, filterWord.Length);

                        int distance = filterWord.EditDistanceTo(subModWord, Thread.CurrentThread.CurrentCulture, CompareOptions.IgnoreCase);
                        if (distance <= Math.Ceiling(filterWord.Length * 0.2)) return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a text contains a filter pattern.
        /// A heuristic function is used to determine if the filter pattern is contained.
        /// </summary>
        /// <param name="filter">The filter pattern.</param>
        /// <param name="text">The text to check on.</param>
        /// <returns>Returns true if the text appears to contain the filter pattern, otherwise false.</returns>
        public static bool FilterIsContained(string filter, string text)
        {
            char[] whitespaceChars = { '-', '_', '.', ':', ',', ';', '=', '?', '!', '(', ')', '[', ']', '{', '}', '+', '/', '\\', '&', '|', '<', '>' };
            char[] nullChars = { '\'', '\"', '*', '~', '#', '%', '$', '§', '^', '°' };

            filter = filter.Replace(whitespaceChars, ' ').Replace(nullChars, null);
            string[] filterWords = filter.SplitOnWhitespace();

            text = text.Replace(whitespaceChars, ' ').Replace(nullChars, null);
            IEnumerable<string> textWords = text.SplitOnWhitespace();

            return filterWords.All(filterWord => FilterWordIsContained(filterWord, textWords));
        }
    }
}