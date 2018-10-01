using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents the Steam version of Factorio.
    /// </summary>
    sealed class FactorioSteamVersion : FactorioVersion
    {
        const int AppId = 427520;

        public static string SteamAppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");

        /// <summary>
        /// Tries to ready the Steam installation path from the Registry.
        /// </summary>
        /// <param name="path">Out. If the action was sucessful, the full path to the Steam installation directory.</param>
        /// <returns>Return true if the path was found in the Registry, otherwise false.</returns>
        public static bool TryGetSteamInstallPath(out string path)
        {
            RegistryKey softwareKey = null;
            try
            {
                string softwarePath = Environment.Is64BitProcess ? @"SOFTWARE\WOW6432Node" : "SOFTWARE";
                softwareKey = Registry.LocalMachine.OpenSubKey(softwarePath, false);

                using (var key = softwareKey.OpenSubKey(@"Valve\Steam"))
                {
                    var obj = key.GetValue("InstallPath");
                    path = obj as string;
                    return !string.IsNullOrWhiteSpace(path);
                }
            }
            catch
            {
                path = null;
                return false;
            }
            finally
            {
                if (softwareKey != null)
                    softwareKey.Close();
            }
        }

        /// <summary>
        /// Checks if Steam is installed on the system.
        /// </summary>
        /// <returns>Returns true if Steam is installed on the system, otherwise false.</returns>
        public static bool IsSteamInstalled()
        {
            return TryGetSteamInstallPath(out var path);
        }

        /// <summary>
        /// Tries to load the Steam version of Factorio specified in the settings.
        /// </summary>
        /// <param name="steamVersion">Out. The Steam version.</param>
        /// <returns>Returns true if the Steam version has been loaded sucessfully, otherwise false.</returns>
        public static bool TryLoad(out FactorioVersion steamVersion)
        {
            if (string.IsNullOrEmpty(App.Instance.Settings.SteamVersionPath))
            {
                steamVersion = null;
                return false;
            }

            var directory = new DirectoryInfo(App.Instance.Settings.SteamVersionPath);
            if (FactorioFolder.TryLoad(directory, out var folder))
            {
                steamVersion = new FactorioSteamVersion(folder);
                return true;
            }
            else
            {
                steamVersion = null;
                return false;
            }
        }

        protected override string LoadName()
        {
            return "Steam";
        }

        public override void Run(string args = null)
        {
            string steamPath;
            if (!TryGetSteamInstallPath(out steamPath)) return;

            var startInfo = new ProcessStartInfo(Path.Combine(steamPath, "Steam.exe"));
            startInfo.Arguments = $"-applaunch {AppId}";
            if (!string.IsNullOrWhiteSpace(args)) startInfo.Arguments += $" {args}";

            Process.Start(startInfo);
        }

        private FactorioSteamVersion(FactorioFolder folder)
            : base(folder, false, new DirectoryInfo(SteamAppDataPath))
        { }
    }
}
