using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.Models.ModSettings;
using ModMyFactory.ModSettings.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModMyFactory.ModSettings
{
    static class ModSettingsManager
    {
        static readonly Version BehaviourSwitch = new Version(0, 16); // 0.15 uses json format while 0.16 onwards use binary format.

        static int updateCount = 0;
        static readonly FileInfo settingsFile;
        static Dictionary<string, ModSettingsExportTemplate> modSettings;
        static Dictionary<Version, BinaryFile> binaryFiles;
        static Dictionary<Version, ModSettingsExportTemplate> deserializedBinary;

        static ModSettingsManager()
        {
            settingsFile = new FileInfo(Path.Combine(App.Instance.AppDataPath, "mod-settings.json"));
        }

        private static ModSettingsExportTemplate GetTemplate(Version version)
        {
            if (!deserializedBinary.TryGetValue(version, out var result))
            {
                result = new ModSettingsExportTemplate();
                deserializedBinary.Add(version, result);
            }
            return result;
        }

        public static void BeginUpdate()
        {
            updateCount++;
        }

        public static void EndUpdate(bool force = false)
        {
            if (force) updateCount = 0;
            else updateCount--;
        }
        
        public static void SaveBinarySettings(ModCollection mods)
        {
            if (updateCount > 0) return;

            binaryFiles.Clear();
            deserializedBinary.Clear();

            foreach (var mod in mods)
            {
                if (mod.Active)
                {
                    var binary = GetTemplate(mod.FactorioVersion);
                    binary.AddMod(mod);
                }
            }

            var modDir = App.Instance.Settings.GetModDirectory();
            foreach (var subDir in modDir.EnumerateDirectories())
            {
                if (Version.TryParse(subDir.Name, out var version))
                    GetTemplate(version);
            }

            foreach (var kvp in deserializedBinary)
            {
                var version = kvp.Key;
                var template = kvp.Value;
                
                if (version >= BehaviourSwitch)
                {
                    string json = JsonHelper.Serialize(template);

                    var file = new BinaryFile(version, json);
                    var dir = App.Instance.Settings.GetModDirectory(version);
                    if (!dir.Exists) dir.Create();
                    file.Save(new FileInfo(Path.Combine(dir.FullName, "mod-settings.dat")));
                }
                else
                {
                    var dir = App.Instance.Settings.GetModDirectory(version);
                    if (!dir.Exists) dir.Create();
                    JsonHelper.Serialize(template, new FileInfo(Path.Combine(dir.FullName, "mod-settings.json")));
                }
            }
        }

        public static void SaveBinarySettings(Modpack modpack)
        {
            if (updateCount > 0) return;
        }

        public static void LoadSettings()
        {
            if (settingsFile.Exists)
            {
                try
                {
                    modSettings = JsonHelper.Deserialize<Dictionary<string, ModSettingsExportTemplate>>(settingsFile);
                }
                catch (JsonSerializationException ex)
                {
                    App.Instance.WriteExceptionLog(ex);
                    settingsFile.Delete();
                    modSettings = new Dictionary<string, ModSettingsExportTemplate>();
                }
            }
            else
            {
                modSettings = new Dictionary<string, ModSettingsExportTemplate>();
            }

            binaryFiles = new Dictionary<Version, BinaryFile>();
            deserializedBinary = new Dictionary<Version, ModSettingsExportTemplate>();
            var modDir = App.Instance.Settings.GetModDirectory();
            foreach (var subDir in modDir.EnumerateDirectories())
            {
                if (Version.TryParse(subDir.Name, out var version))
                {
                    if (version >= BehaviourSwitch)
                    {
                        var file = new FileInfo(Path.Combine(subDir.FullName, "mod-settings.dat"));
                        if (file.Exists)
                        {
                            try
                            {
                                var binFile = new BinaryFile(file);
                                binaryFiles.Add(version, binFile);

                                var template = JsonHelper.Deserialize<ModSettingsExportTemplate>(binFile.JsonString);
                                deserializedBinary.Add(version, template);
                            }
                            catch (Exception ex) when (ex is ArgumentException || ex is JsonSerializationException)
                            {
                                App.Instance.WriteExceptionLog(ex);
                            }
                        }
                    }
                    else
                    {
                        var file = new FileInfo(Path.Combine(subDir.FullName, "mod-settings.json"));
                        if (file.Exists)
                        {
                            try
                            {
                                var template = JsonHelper.Deserialize<ModSettingsExportTemplate>(file);
                                deserializedBinary.Add(version, template);
                            }
                            catch (Exception ex) when (ex is ArgumentException || ex is JsonSerializationException)
                            {
                                App.Instance.WriteExceptionLog(ex);
                            }
                        }
                    }
                }
            }
        }
        
        private static bool TryGetSavedBinaryValue<T>(IHasModSettings mod, IModSetting<T> setting, out T value) where T : IEquatable<T>
        {
            if (deserializedBinary.TryGetValue(mod.FactorioVersion, out var settings))
            {
                return settings.TryGetValue(setting, out value);
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public static bool TryGetSavedValue<T>(IHasModSettings mod, IModSetting<T> setting, out T value) where T : IEquatable<T>
        {
            if (mod.UseBinaryFileOverride && TryGetSavedBinaryValue(mod, setting, out value)) return true;
            
            bool result = modSettings.TryGetValue(mod.UniqueID, out var settings);
            if (result)
            {
                return settings.TryGetValue(setting, out value);
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public static bool HasSavedDataPresent(IHasModSettings mod)
        {
            if (modSettings == null) return false;
            return modSettings.ContainsKey(mod.UniqueID);
        }

        private static void AddSettings(IHasModSettings mod)
        {
            if (mod.HasSettings && mod.Override)
            {
                var exportTemplate = new ModSettingsExportTemplate(mod);
                modSettings[mod.UniqueID] = exportTemplate;
            }
            else
            {
                if (modSettings.ContainsKey(mod.UniqueID))
                    modSettings.Remove(mod.UniqueID);
            }
        }

        public static void SaveSettings(IHasModSettings mod)
        {
            AddSettings(mod);
            JsonHelper.Serialize(modSettings, settingsFile);
        }

        public static void SaveSettings(IEnumerable<IHasModSettings> mods)
        {
            foreach (var mod in mods) AddSettings(mod);
            JsonHelper.Serialize(modSettings, settingsFile);
        }

        public static void RemoveSettings(IHasModSettings mod)
        {
            if (modSettings.ContainsKey(mod.UniqueID))
                modSettings.Remove(mod.UniqueID);

            JsonHelper.Serialize(modSettings, settingsFile);
        }
    }
}
