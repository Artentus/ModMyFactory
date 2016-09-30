using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Models
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

            CreateSaveDirectoryLink();
            CreateScenarioDirectoryLink();
            CreateModDirectoryLink(false);
        }

        public void CreateSaveDirectoryLink()
        {
            DirectoryInfo localSaveDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "saves"));
            if (!localSaveDirectory.Exists)
            {
                var globalSaveDirectory = new DirectoryInfo(Path.Combine(App.Instance.AppDataPath, "saves"));
                if (!globalSaveDirectory.Exists) globalSaveDirectory.Create();

                var info = new ProcessStartInfo("cmd")
                {
                    Arguments = $"/K mklink /J \"{localSaveDirectory.FullName}\" \"{globalSaveDirectory.FullName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(info);
            }
        }

        public void CreateScenarioDirectoryLink()
        {
            DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "scenarios"));
            if (!localScenarioDirectory.Exists)
            {
                var globalScenarioDirectory = new DirectoryInfo(Path.Combine(App.Instance.AppDataPath, "scenarios"));
                if (!globalScenarioDirectory.Exists) globalScenarioDirectory.Create();

                var info = new ProcessStartInfo("cmd")
                {
                    Arguments = $"/K mklink /J \"{localScenarioDirectory.FullName}\" \"{globalScenarioDirectory.FullName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(info);
            }
        }

        public void CreateModDirectoryLink(bool forced)
        {
            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "mods"));
            if (forced && localModDirectory.Exists) localModDirectory.Delete(false);

            if (!localModDirectory.Exists)
            {
                var globalModDirectory = App.Instance.Settings.GetModDirectory(Version);
                if (!globalModDirectory.Exists) globalModDirectory.Create();

                var info = new ProcessStartInfo("cmd")
                {
                    Arguments = $"/K mklink /J \"{localModDirectory.FullName}\" \"{globalModDirectory.FullName}\"",
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

            DirectoryInfo localScenarioDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "scenarios"));
            if (localScenarioDirectory.Exists) localScenarioDirectory.Delete(false);

            DirectoryInfo localModDirectory = new DirectoryInfo(Path.Combine(Directory.FullName, "mods"));
            if (localModDirectory.Exists) localModDirectory.Delete(false);
        }
    }
}
