using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ModMyFactory.Helpers
{
    static class StringExtensions
    {
        /// <summary>
        /// Wraps this string on a specified with.
        /// </summary>
        /// <param name="columnWidth">The width the string gets wraped at.</param>
        public static string Wrap(this string value, int columnWidth)
        {
            string[] words = value.Split(' ');

            var wrappedText = new StringBuilder();

            var line = "";
            foreach (var word in words)
            {
                if ((line + word).Length > columnWidth)
                {
                    wrappedText.AppendLine(line);
                    line = "";
                }

                line += $"{word} ";
            }

            if (line.Length > 0)
            {
                wrappedText.Append(line);
            }

            return wrappedText.ToString();
        }

        /// <summary>
        /// Splits this string on whitespace.
        /// </summary>
        public static string[] SplitOnWhitespace(this string value)
        {
            return value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Calculates the edit distance to another string.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <param name="culture">The culture that is used to compare the strings.</param>
        /// <param name="options">A value that spoecifies how the two string are compared.</param>
        /// <returns>Returns the minimum edit distance between this string and the second string.</returns>
        public static int EditDistanceTo(this string first, string second, CultureInfo culture, CompareOptions options)
        {
            return StringHelper.GetEditDistance(first, second, culture, options);
        }

        /// <summary>
        /// Calculates the edit distance to another string.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <param name="culture">The culture that is used to compare the strings.</param>
        /// <returns>Returns the minimum edit distance between this string and the second string.</returns>
        public static int EditDistanceTo(this string first, string second, CultureInfo culture)
        {
            return StringHelper.GetEditDistance(first, second, culture);
        }

        /// <summary>
        /// Calculates the edit distance to another string.
        /// </summary>
        /// <param name="first">The first string.</param>
        /// <param name="second">The second string.</param>
        /// <returns>Returns the minimum edit distance between this string and the second string.</returns>
        public static int EditDistanceTo(this string first, string second)
        {
            return StringHelper.GetEditDistance(first, second);
        }

        /// <summary>
        /// Returns a new string in which all occurrences of the specified Unicode characters in this instance are replaced with another specified Unicode character.
        /// </summary>
        /// <param name="oldChars">The Unicode characters to be replaced.</param>
        /// <param name="newChar">The Unicode character to replace all occurrences of the old characters.</param>
        /// <returns>A string that is equivalent to this instance except that all instances of the old characters are replaced with newChar. If the old characters are not found in the current instance, the method returns the current instance unchanged.</returns>
        public static string Replace(this string value, char[] oldChars, char? newChar)
        {
            var sb = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (oldChars.Contains(c))
                {
                    if (newChar.HasValue)
                        sb.Append(newChar.Value);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
