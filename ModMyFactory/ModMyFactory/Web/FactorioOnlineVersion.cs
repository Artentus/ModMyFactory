using System;
using WPFCore;

namespace ModMyFactory.Web
{
    sealed class FactorioOnlineVersion : NotifyPropertyChangedBase
    {
        private static string GetBranch(Version version)
        {
            if (version.Major == 0) return "alpha";

            return string.Empty; // Unknown yet, wait for 1.0 release
        }


        public Version Version { get; }
        
        public bool IsExperimental { get; }

        public string DownloadUrl { get; }

        public FactorioOnlineVersion(Version version, bool isExperimental)
        {
            Version = version;
            IsExperimental = isExperimental;

            string versionString = version.ToString(3);
            string branch = GetBranch(version);
            string platformString = Environment.Is64BitOperatingSystem ? "win64-manual" : "win32-manual";
            DownloadUrl = $"https://www.factorio.com/get-download/{versionString}/{branch}/{platformString}";
        }
    }
}
