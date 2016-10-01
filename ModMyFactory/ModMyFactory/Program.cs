using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

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
            App app = null;

            // Custom AppData path for debugging purposes only.
            int index = Array.IndexOf(args, "-a");
            if (index >= 0)
            {
                index++;

                if (index < args.Length)
                {
                    string appDataPath = args[index];
                    app = new App(appDataPath);
                }
            }
            if (app == null) app = new App();

            // Prevent update search on startup
            index = Array.IndexOf(args, "-u");
            UpdateCheckOnStartup = (index < 0);

            // Direct game start logic.
            index = Array.IndexOf(args, "-v");
            if (index >= 0)
            {
                index++;

                Version version;
                if (index < args.Length && Version.TryParse(args[index], out version))
                {
                    var versions = FactorioVersion.GetInstalledVersions();
                    FactorioVersion factorioVersion = versions.Find(item => item.Version == version);
                    if (factorioVersion != null)
                    {
                        var mods = new List<Mod>();
                        var modpacks = new List<Modpack>();

                        Mod.LoadTemplates();
                        Mod.BeginUpdateTemplates();
                        Mod.LoadMods(mods, modpacks, null);
                        var modpackTemplateList =
                            ModpackTemplateList.Load(Path.Combine(app.AppDataPath, "modpacks.json"));
                        modpackTemplateList.PopulateModpackList(mods, modpacks, null, null);

                        mods.ForEach(mod => mod.Active = false);

                        index = Array.IndexOf(args, "-p");
                        if (index >= 0)
                        {
                            index++;

                            Modpack modpack = modpacks.FirstOrDefault(item => item.Name == args[index]);
                            if (index < args.Length && modpack != null)
                            {
                                modpack.Active = true;
                            }
                        }

                        Mod.EndUpdateTemplates(true);
                        Mod.SaveTemplates();

                        Process.Start(factorioVersion.ExecutablePath);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Factorio version {version.ToString(3)} is not available.\nCheck your installed Factorio versions.",
                            "Error starting game!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                return 0;
            }

            app.InitializeComponent();
            app.Run();

            return 0;
        }
    }
}
