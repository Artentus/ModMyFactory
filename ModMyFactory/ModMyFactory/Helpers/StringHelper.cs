using System.Globalization;
using System.Linq;

namespace ModMyFactory.Helpers
{
    static class StringHelper
    {
        private static int[,] CreateTable(int width, int height)
        {
            int[,] table = new int[width, height];
            for (int x = 0; x < width; x++) table[x, 0] = x;
            for (int y = 0; y < height; y++) table[0, y] = y;

            return table;
        }

        private static int GetTableMinimum(int[,] table, int x, int y)
        {
            int[] values = new int[3];
            values[0] = table[x - 1, y - 1];
            values[1] = table[x, y - 1];
            values[2] = table[x - 1, y];

            return values.Min();
        }

        /// <summary>
        /// Calculates the edit distance of two strings.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <param name="culture">The culture that is used to compare the strings.</param>
        /// <param name="options">A value that spoecifies how the two string are compared.</param>
        /// <returns>Returns the minimum edit distance between the two input strings.</returns>
        public static int GetEditDistance(string first, string second, CultureInfo culture, CompareOptions options)
        {
            if (first.Length == 0) return second.Length;
            if (second.Length == 0) return first.Length;

            int[,] table = CreateTable(first.Length + 1, second.Length + 1);

            for (int x = 1; x <= first.Length; x++)
            {
                string cx = first.Substring(x - 1, 1);

                for (int y = 1; y <= second.Length; y++)
                {
                    string cy = second.Substring(y - 1, 1);

                    if (culture.CompareInfo.Compare(cx, cy, options) == 0)
                    {
                        table[x, y] = table[x - 1, y - 1];
                    }
                    else
                    {
                        table[x, y] = GetTableMinimum(table, x, y) + 1;
                    }
                }
            }

            return table[first.Length, second.Length];
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
    }
}