using System.Text;

namespace ModMyFactory.Helpers
{
    public class StringHelper
    {
        public static string Wrap(string text, int columnWidth)
        {
            string[] words = text.Split(' ');

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
    }
}