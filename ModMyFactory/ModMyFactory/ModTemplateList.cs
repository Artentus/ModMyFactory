using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ModMyFactory.Helpers;
using ModMyFactory.Models;

namespace ModMyFactory
{
    /// <summary>
    /// Represents a mod-list.json file.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    sealed class ModTemplateList
    {
        /// <summary>
        /// Data object for serialization.
        /// </summary>
        [JsonObject(MemberSerialization.OptOut)]
        sealed class ModTemplate
        {
            [JsonProperty(PropertyName = "name")]
            public readonly string Name;

            [JsonProperty(PropertyName = "enabled")]
            [JsonConverter(typeof(BooleanToStringJsonConverter))]
            public bool Enabled;

            [JsonProperty(PropertyName = "version", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [JsonConverter(typeof(GameVersionConverter))]
            public GameCompatibleVersion Version;

            [JsonConstructor]
            public ModTemplate(string name, bool enabled, GameCompatibleVersion version)
            {
                Name = name;
                Enabled = enabled;
                Version = version;
            }
        }

        /// <summary>
        /// Loads a specified mod-list.json file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>Returns a ModTemplateList representing the specified mod-list.json file.</returns>
        public static ModTemplateList Load(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists)
            {
                ModTemplateList templateList = JsonHelper.Deserialize<ModTemplateList>(file);
                templateList.file = file;
                return templateList;
            }
            else
            {
                var templateList = new ModTemplateList(file);
                templateList.Save();
                return templateList;
            }
        }
        
        FileInfo file;
        bool updating;
        int updateCount;

        [JsonProperty(PropertyName = "mods")]
        List<ModTemplate> Mods;

        public Version Version { get; set; }

        [JsonConstructor]
        private ModTemplateList()
        { }

        private ModTemplateList(FileInfo file)
        {
            this.file = file;

            Mods = new List<ModTemplate>();
            Mods.Add(new ModTemplate("base", true, null));
        }

        private ModTemplate CreateTemplate(string name, bool enabled, GameCompatibleVersion version, Version factorioVersion)
        {
            if (factorioVersion >= FactorioVersion.DisableBehaviourSwitch)
            {
                return new ModTemplate(name, enabled, version);
            }
            else
            {
                return new ModTemplate(name, enabled, null);
            }
        }
        
        private bool TryGetMod(string name, out ModTemplate mod)
        {
            mod = Mods.Find(item => item.Name == name);
            return mod != default(ModTemplate);
        }

        /// <summary>
        /// Gets the active state of a mod.
        /// </summary>
        /// <param name="name">The mods name.</param>
        /// <returns>Returns if the specified mod is active.</returns>
        public bool GetActive(string name, GameCompatibleVersion version, Version factorioVersion)
        {
            if (TryGetMod(name, out var mod))
            {
                if (factorioVersion >= FactorioVersion.DisableBehaviourSwitch)
                {
                    return mod.Enabled && (mod.Version == version);
                }
                else
                {
                    return mod.Enabled;
                }
            }
            else
            {
                mod = CreateTemplate(name, App.Instance.Settings.ActivateNewMods, version, factorioVersion);
                Mods.Add(mod);
                Save();
                return mod.Enabled;
            }
        }

        /// <summary>
        /// Sets the active state of a mod.
        /// </summary>
        /// <param name="name">The mods name.</param>
        /// <param name="value">The new active state of the mod.</param>
        public void SetActive(string name, bool value, GameCompatibleVersion version, Version factorioVersion)
        {
            if (TryGetMod(name, out var mod))
            {
                if (factorioVersion >= FactorioVersion.DisableBehaviourSwitch)
                {
                    if (value || (mod.Version == version))
                    {
                        mod.Enabled = value;
                        mod.Version = version;
                    }
                }
                else
                {
                    mod.Enabled = value;
                }
            }
            else
            {
                mod = CreateTemplate(name, value, version, factorioVersion);
                Mods.Add(mod);
            }

            Save();
        }

        /// <summary>
        /// Removes a mods template.
        /// </summary>
        /// <param name="name">The mods name.</param>
        public void Remove(string name)
        {
            if (TryGetMod(name, out var mod))
            {
                Mods.Remove(mod);
                Save();
            }
        }

        /// <summary>
        /// Starts updating this ModTemplateList.
        /// While updating all save commands will be ignored.
        /// </summary>
        public void BeginUpdate()
        {
            updating = true;
            updateCount++;
        }

        /// <summary>
        /// Finishes updating this ModTemplateList.
        /// </summary>
        /// <param name="force">If true forces to end updating, otherwise EndUpdate will have to be called as many times as BeginUpdate has been called.</param>
        public void EndUpdate(bool force)
        {
            if (force)
            {
                updateCount = 0;
                updating = false;
            }
            else
            {
                if (updateCount > 0) updateCount--;
                if (updateCount == 0) updating = false;
            }
        }

        /// <summary>
        /// Saves this ModTemplateList to its file.
        /// </summary>
        public void Save()
        {
            if (!updating)
            {
                JsonHelper.Serialize(this, file);
            }
        }
    }
}
