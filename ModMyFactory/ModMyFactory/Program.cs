using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ModMyFactory.Helpers;
using ModMyFactory.Win32;

namespace ModMyFactory
{
    public static class Program
    {
        /// <summary>
        /// Indicates whether ModMyFatory should check for updates on startup.
        /// </summary>
        public static bool UpdateCheckOnStartup { get; private set; }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            var commandLine = new CommandLine(args);

            // Only display help.
            if (commandLine.IsSet('h', "help"))
            {
                bool attatchedConsole = Kernel32.AttachConsole();
                if (attatchedConsole)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                }
                else
                {
                    Kernel32.AllocConsole();
                }

                Console.WriteLine("Usage:");
                Console.WriteLine("  modmyfactory.exe -h | --help");
                Console.WriteLine("  modmyfactory.exe [options]");
                Console.WriteLine("  modmyfactory.exe [options] -f <version> | --factorio-version=<version> [(-p <name> | --modpack=<name>)]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help                                 Display this help message.");
                Console.WriteLine("  -l, --no-logs                              Don't create crash logs.");
                Console.WriteLine("  -a PATH, --appdata-path=PATH               Overwrite the default application data path.");
                Console.WriteLine("  -u, --no-update                            Don't search for update on startup.");
                Console.WriteLine("  -f VERSION, --factorio-version=VERSION     Start the specified version of Factorio.");
                Console.WriteLine("  -p NAME, --modpack=NAME                    Enable the specified modpack.");

                if (attatchedConsole)
                {
                    System.Windows.Forms.SendKeys.SendWait("{Enter}");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
                }
                Kernel32.FreeConsole();

                return 0;
            }

            // Do not create crash logs when debugging.
            bool createCrashLog = !commandLine.IsSet('l', "no-logs");

            // Custom AppData path for debugging purposes only.
            App app = null;
            string appDataPath;
            if (commandLine.TryGetArgument('a', "appdata-path", out appDataPath))
                app = new App(createCrashLog, appDataPath);
            else
                app = new App(createCrashLog);

            // Prevent update search on startup
            UpdateCheckOnStartup = !commandLine.IsSet('u', "no-update");

            // Direct game start logic.
            string versionString;
            if (commandLine.TryGetArgument('f', "factorio-version", out versionString))
            {
                var versions = FactorioVersion.GetInstalledVersions();
                FactorioVersion steamVersion;
                if (FactorioSteamVersion.TryLoad(out steamVersion)) versions.Add(steamVersion);

                FactorioVersion factorioVersion = null;
                if (string.Equals(versionString, FactorioVersion.LatestKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    factorioVersion = versions.MaxBy(item => item.Version, new VersionComparer());
                }
                else
                {
                    factorioVersion = versions.Find(item => item.VersionString == versionString);
                }

                if (factorioVersion != null)
                {
                    var mods = new List<Mod>();
                    var modpacks = new List<Modpack>();

                    ModManager.BeginUpdateTemplates();
                    Mod.LoadMods(mods, modpacks, null);
                    ModpackTemplateList.Instance.PopulateModpackList(mods, modpacks, null, null);

                    mods.ForEach(mod => mod.Active = false);

                    string modpackName;
                    if (commandLine.TryGetArgument('p', "modpack", out modpackName))
                    {
                        Modpack modpack = modpacks.FirstOrDefault(item => item.Name == modpackName);
                        if (modpack != null)
                        {
                            modpack.Active = true;
                        }
                        else
                        {
                            MessageBox.Show(
                                $"No modpack named '{modpackName}' found.\nThe game will be launched without any mods enabled.",
                                "Error loading modpack!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    ModManager.EndUpdateTemplates(true);
                    ModManager.SaveTemplates();

                    Process.Start(factorioVersion.ExecutablePath);
                }
                else
                {
                    MessageBox.Show(
                        $"Factorio version {versionString} is not available.\nCheck your installed Factorio versions.",
                        "Error starting game!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return 0;
            }

            app.InitializeComponent();
            app.Run();

            return 0;
        }
    }
}
