using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ModMyFactory.Models;
using ModMyFactory.Helpers;

namespace ModMyFactory
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class ModpackTemplateList
    {
        [JsonObject(MemberSerialization.OptOut)]
        sealed class ModpackTemplateMod
        {
            public string Name { get; }

            [JsonConverter(typeof(VersionConverter))]
            public Version FactorioVersion { get; }

            [JsonConstructor]
            public ModpackTemplateMod(string name, Version factorioVersion)
            {
                Name = name;
                FactorioVersion = factorioVersion;
            }
        }

        [JsonObject(MemberSerialization.OptOut)]
        sealed class ModpackTemplate
        {
            public string Name { get; set; }

            public ModpackTemplateMod[] Mods { get; set; }

            public string[] Modpacks { get; set; }
        }

        static ModpackTemplateList instance;

        public static ModpackTemplateList Instance => instance ?? (instance = Load(Path.Combine(App.Instance.AppDataPath, "modpacks.json")));

        /// <summary>
        /// Loads modpack templates from a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>Returns a ModpackTemplateList representing the specified file.</returns>
        private static ModpackTemplateList Load(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists)
            {
                ModpackTemplateList templateList = JsonHelper.Deserialize<ModpackTemplateList>(file);
                templateList.file = file;
                return templateList;
            }
            else
            {
                var templateList = new ModpackTemplateList(file);
                templateList.Save();
                return templateList;
            }
        }

        FileInfo file;

        [JsonProperty]
        ModpackTemplate[] Modpacks;

        [JsonConstructor]
        private ModpackTemplateList()
        { }

        private ModpackTemplateList(FileInfo file)
        {
            this.file = file;

            Modpacks = new ModpackTemplate[0];
        }

        private Mod GetMod(ICollection<Mod> modList, ModpackTemplateMod modTemplate)
        {
            return modList.FirstOrDefault(mod => mod.Title == modTemplate.Name
                                                 && mod.FactorioVersion == modTemplate.FactorioVersion);
        }

        private Modpack GetModpack(ICollection<Modpack> modpackList, string name)
        {
            return modpackList.FirstOrDefault(modpack => modpack.Name == name);
        }

        private ModpackTemplate GetTemplate(string name)
        {
            return Modpacks.First(template => template.Name == name);
        }

        public void PopulateModpackList(ICollection<Mod> modList, ICollection<Modpack> modpackList, IEditableCollectionView modpackView, Window messageOwner)
        {
            foreach (var template in Modpacks)
            {
                var modpack = new Modpack(template.Name, modpackList, messageOwner);
                if (modpackView != null) modpack.ParentView = modpackView;

                foreach (ModpackTemplateMod modTemplate in template.Mods)
                {
                    Mod mod = GetMod(modList, modTemplate);
                    if (mod != null) modpack.Mods.Add(new ModReference(mod, modpack));
                }

                modpackList.Add(modpack);
            }

            foreach (var modpack in modpackList)
            {
                ModpackTemplate template = GetTemplate(modpack.Name);

                foreach (string modpackName in template.Modpacks)
                {
                    Modpack subModpack = GetModpack(modpackList, modpackName);
                    var reference = new ModpackReference(subModpack, modpack);
                    reference.ParentView = modpack.ModsView;
                    if (subModpack != null) modpack.Mods.Add(reference);
                }
            }
        }

        public void Update(ICollection<Modpack> modpackList)
        {
            Modpacks = new ModpackTemplate[modpackList.Count];

            int index = 0;
            foreach (var modpack in modpackList)
            {
                Modpacks[index] = new ModpackTemplate()
                {
                    Name = modpack.Name,
                    Mods = modpack.Mods.Where(item => item is ModReference)
                    .Select(item => new ModpackTemplateMod(item.DisplayName, ((ModReference)item).Mod.FactorioVersion)).ToArray(),
                    Modpacks = modpack.Mods.Where(item => item is ModpackReference).Select(item => item.DisplayName).ToArray(),
                };

                index++;
            }
        }

        /// <summary>
        /// Saves this ModpackTemplateList to its file.
        /// </summary>
        public void Save()
        {
            JsonHelper.Serialize(this, file);
        }
    }
}
