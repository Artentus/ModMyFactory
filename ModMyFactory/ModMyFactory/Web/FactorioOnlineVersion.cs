using System;
using System.ComponentModel;
using WPFCore;

namespace ModMyFactory.Web
{
    sealed class FactorioOnlineVersion : NotifyPropertyChangedBase
    {
        bool downloadable;

        public Version Version { get; }

        public string VersionModifier { get; }

        public bool IsExperimental { get; }

        public Uri DownloadUrl { get; }

        public bool Downloadable
        {
            get { return downloadable; }
            set
            {
                if (value != downloadable)
                {
                    downloadable = true;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Downloadable)));
                }
            }
        }

        public FactorioOnlineVersion(Version version, string versionModifier, bool isExperimental)
        {
            Version = version;
            VersionModifier = versionModifier;
            IsExperimental = isExperimental;

            string versionString = version.ToString(3);
            string platformString = Environment.Is64BitOperatingSystem ? "win64-manual" : "win32-manual";
            DownloadUrl = new Uri($"https://www.factorio.com/get-download/{versionString}/{versionModifier}/{platformString}");
        }
    }
}
