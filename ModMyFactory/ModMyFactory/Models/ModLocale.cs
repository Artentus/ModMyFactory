using IniParser.Model;
using ModMyFactory.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ModMyFactory.Models
{
    sealed class ModLocale
    {
        readonly IniData data;

        public CultureInfo Culture;

        public ModLocale(CultureInfo culture)
        {
            Culture = culture;
            data = new IniData();
        }

        private static IniData ReadData(IEnumerable<string> contents)
        {
            IniData result = new IniData();
            foreach (var content in contents)
            {
                var data = INIHelper.ReadINI(content);
                result.Merge(data);
            }
            return result;
        }

        public ModLocale(CultureInfo culture, IEnumerable<string> contents)
        {
            Culture = culture;
            data = ReadData(contents);
        }

        public ModLocale(CultureInfo culture, params string[] contents)
            : this(culture, (IEnumerable<string>)contents)
        { }

        private static IniData ReadData(IEnumerable<Stream> streams)
        {
            IniData result = new IniData();
            foreach (var stream in streams)
            {
                var data = INIHelper.ReadINI(stream);
                result.Merge(data);
            }
            return result;
        }

        public ModLocale(CultureInfo culture, IEnumerable<Stream> streams)
        {
            Culture = culture;
            data = ReadData(streams);
        }

        public ModLocale(CultureInfo culture, params Stream[] streams)
            : this(culture, (IEnumerable<Stream>)streams)
        { }

        private static IniData ReadData(IEnumerable<FileInfo> files)
        {
            IniData result = new IniData();
            foreach (var file in files)
            {
                var data = INIHelper.ReadINI(file);
                result.Merge(data);
            }
            return result;
        }

        public ModLocale(CultureInfo culture, IEnumerable<FileInfo> files)
        {
            Culture = culture;
            data = ReadData(files);
        }

        public ModLocale(CultureInfo culture, params FileInfo[] files)
            : this(culture, (IEnumerable<FileInfo>)files)
        { }

        private string GetValue(string section, string key)
        {
            string completeKey = string.Concat(section, data.SectionKeySeparator, key);
            return data.TryGetKey(completeKey, out string result) ? result : null;
        }

        public string GetSettingName(string key)
        {
            const string section = "mod-setting-name";
            return GetValue(section, key);
        }

        public string GetSettingDescription(string key)
        {
            const string section = "mod-setting-description";
            return GetValue(section, key);
        }

        public string GetStringSetting(string key)
        {
            const string section = "string-mod-setting";
            return GetValue(section, key);
        }
    }
}
