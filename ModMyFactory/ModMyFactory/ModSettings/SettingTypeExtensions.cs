namespace ModMyFactory.ModSettings
{
    static class SettingTypeExtensions
    {
        public static bool CompatibleTo(this SettingType value, SettingType other)
        {
            if (value == SettingType.Integer)
            {
                return (other == SettingType.Integer) || (other == SettingType.FloatingPoint);
            }
            else
            {
                return value == other;
            }
        }
    }
}
