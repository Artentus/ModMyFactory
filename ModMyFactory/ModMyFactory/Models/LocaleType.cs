namespace ModMyFactory.Models
{
    enum LocaleType
    {
        None,
        SettingName,
        SettingDescription,
        StringSetting,
    }

    static class LocaleTypeExtensions
    {
        public static string Key(this LocaleType type)
        {
            switch (type)
            {
                case LocaleType.SettingName:
                    return "mod-setting-name";
                case LocaleType.SettingDescription:
                    return "mod-setting-description";
                case LocaleType.StringSetting:
                    return "string-mod-setting";
            }

            return string.Empty;
        }
    }
}
