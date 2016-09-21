using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "saves"));
            if (!localSaveDirectory.Exists)
            {
                string globalSavePath = Path.Combine(App.Instance.AppDataPath, "saves");
                var info = new ProcessStartInfo("cmd")
                {
                    Arguments = $"/K mklink /J \"{localSaveDirectory.FullName}\" \"{globalSavePath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(info);
            }

            CreateModDirectoryLink(false);
        }

        public void CreateModDirectoryLink(bool forced)
        {
            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "mods"));
            if (forced && localModDirectory.Exists) localModDirectory.Delete();

            if (!localModDirectory.Exists)
            {
                string globalSavePath = App.Instance.Settings.GetModDirectory().FullName;
                var info = new ProcessStartInfo("cmd")
                {
                    Arguments = $"/K mklink /J \"{localModDirectory.FullName}\" \"{globalSavePath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(info);
            }
        }

        public void DeleteLinks()
        {
            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "saves"));
            if (localSaveDirectory.Exists) localSaveDirectory.Delete(false);

            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "mods"));
            if (localModDirectory.Exists) localModDirectory.Delete(false);
        }
    }
}
