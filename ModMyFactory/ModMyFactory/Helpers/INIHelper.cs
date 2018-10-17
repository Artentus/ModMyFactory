using System.IO;
using IniParser;
using IniParser.Model;

namespace ModMyFactory.Helpers
{
    static class INIHelper
    {
        public static IniData ReadINI(string content)
        {
            var parser = new StringIniParser();
            return parser.ParseString(content);
        }

        public static IniData ReadINI(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var parser = new StreamIniDataParser();
                return parser.ReadData(reader);
            }
        }

        public static IniData ReadINI(FileInfo file)
        {
            var parser = new FileIniDataParser();
            return parser.ReadFile(file.FullName);
        }
    }
}
