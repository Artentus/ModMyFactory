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

        private static bool IsOptionSpecified(string[] args, char shortName, string longName)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith("-") && arg.Contains(shortName))
                {
                    return true;
                }

                if (arg.StartsWith("--") && (arg.Length == longName.Length + 2)
                    && string.Equals(arg.Substring(2), longName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetOptionArgument(string[] args, char? shortName, string longName, out string argument)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (shortName.HasValue)
                {
                    if (arg.StartsWith("-") && arg.Contains(shortName.Value))
                    {
                        argument = (i + 1 < args.Length) ? args[i + 1] : string.Empty;
                        return true;
                    }
                }

                if (arg.StartsWith("--") && (arg.Length >= longName.Length + 2)
                    && string.Equals(arg.Substring(2, longName.Length), longName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if ((arg.Length > longName.Length + 2))
                    {
                        if (arg[longName.Length + 2] == '=')
                        {
                            argument = (arg.Length > longName.Length + 3) ? arg.Substring(longName.Length + 3) : string.Empty;
                            return true;
                        }
                    }
                    else
                    {
                        argument = string.Empty;
                        return true;
                    }
                }
            }

            argument = null;
            return false;
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            // Only display help.
            if (IsOptionSpecified(args, 'h', "help"))
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
            bool createCrashLog = !IsOptionSpecified(args, 'l', "no-logs");

            // Custom AppData path for debugging purposes only.
            App app = null;
            string appDataPath;
            if (TryGetOptionArgument(args, 'a', "appdata-path", out appDataPath))
                app = new App(createCrashLog, appDataPath);
            else
                app = new App(createCrashLog);

            // Prevent update search on startup
            UpdateCheckOnStartup = !IsOptionSpecified(args, 'u', "no-update");

            // Direct game start logic.
            string versionString;
            if (TryGetOptionArgument(args, 'f', "factorio-version", out versionString))
            {
                FactorioVersion factorioVersion = null;
                if (string.Equals(versionString, "latest", StringComparison.InvariantCultureIgnoreCase))
                {
                    var versions = FactorioVersion.GetInstalledVersions();
                    factorioVersion = versions.MaxBy(item => item.Version, new VersionComparer());
                }
                else
                {
                    Version version;
                    if (Version.TryParse(versionString, out version))
                    {
                        var versions = FactorioVersion.GetInstalledVersions();
                        factorioVersion = versions.Find(item => item.Version == version);
                    }
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
                    if (TryGetOptionArgument(args, 'p', "modpack", out modpackName))
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
