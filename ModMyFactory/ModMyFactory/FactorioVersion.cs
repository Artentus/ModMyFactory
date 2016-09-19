using System;
using System.Collections.Generic;
using System.IO;

namespace ModMyFactory
{
    class FactorioVersion
    {
        public static List<FactorioVersion> GetInstalledVersions()
        {
            var versionList = new List<FactorioVersion>();

            DirectoryInfo factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
            if (factorioDirectory.Exists)
            {
                foreach (var directory in factorioDirectory.EnumerateDirectories())
                {
                    Version version;
                    bool result = Version.TryParse(directory.Name, out version);

                    if (result)
                    {
                        var factorioVersion = new FactorioVersion(directory, version);
                        versionList.Add(factorioVersion);
                    }
                }
            }

            return versionList;
        }

        public Version Version { get; }

        public string DisplayName => "Factorio " + Version.ToString(3);

        public DirectoryInfo Directory { get; }

        public string ExecutablePath { get; }

        public FactorioVersion(DirectoryInfo directory, Version version)
        {
            Version = version;
            Directory = directory;

            string osPlatform = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            ExecutablePath = Path.Combine(directory.FullName, "bin", osPlatform, "factorio.exe");
        }
    }
}
