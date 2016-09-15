using System;

namespace ModMyFactory.Web
{
    class FactorioOnlineVersion
    {
        public Version Version { get; }

        public string VersionModifier { get; }

        public Uri DownloadUrl { get; }

        public FactorioOnlineVersion(Version version, string versionModifier)
        {
            Version = version;
            VersionModifier = versionModifier;

            string versionString = version.ToString(3);
            int startIndex = versionModifier.IndexOf('(') + 1;
            int endIndex = versionModifier.IndexOf(')');
            string modifierUrlPart = versionModifier.Substring(startIndex, endIndex - startIndex);
            string platformString = Environment.Is64BitOperatingSystem ? "win64-manual" : "win32-manual";
            DownloadUrl = new Uri($"https://www.factorio.com/get-download/{versionString}/{modifierUrlPart}/{platformString}");
        }
    }
}
