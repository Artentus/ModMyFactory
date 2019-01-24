using System;
using System.Collections.Generic;
using System.ComponentModel;
using ModMyFactory.Models;
using Newtonsoft.Json;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ModpackExportTemplate : IEquatable<ModpackExportTemplate>
    {
        //---------------------------------------------------- Deprecated ------------------------------------------------------------

        public static ModpackExportTemplate FromModpack(Modpack modpack, bool includeVersionInfo)
        {
            var mods = new List<ModExportTemplate>();
            var modpacks = new List<string>();

            foreach (var reference in modpack.Mods)
            {
                ModReference modReference = reference as ModReference;
                ModpackReference modpackReference = reference as ModpackReference;

                if (modReference != null)
                {
                    mods.Add(ModExportTemplate.FromMod(modReference.Mod, includeVersionInfo));
                }
                else if (modpackReference != null)
                {
                    modpacks.Add(modpackReference.Modpack.Name);
                }
            }

            return new ModpackExportTemplate(modpack.Name, mods.ToArray(), modpacks.ToArray(), modpack.ModSettings);
        }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ModExportTemplate[] Mods { get; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Modpacks { get; }


        private ModpackExportTemplate(string name, ModExportTemplate[] mods, string[] modpacks, string modSettings)
            : this(-1, name, mods, modpacks, null, null, modSettings)
        { }

        //----------------------------------------------------------------------------------------------------------------------------




        private static int globalUid = 0;

        public static void ResetUid()
        {
            globalUid = 0;
        }



        [JsonIgnore]
        public Modpack Modpack { get; set; }

        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Uid { get; }

        public string Name { get; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int[] ModIds { get; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int[] ModpackIds { get; }
        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ModSettings { get; }

        [JsonConstructor]
        private ModpackExportTemplate(int uid, string name, ModExportTemplate[] mods, string[] modpacks, int[] modIds, int[] modpackIds, string modSettings)
        {
            Uid = uid;
            Name = name;
            Mods = mods;
            Modpacks = modpacks;
            ModIds = modIds;
            ModpackIds = modpackIds;
           ModSettings = modSettings;
        }

        public ModpackExportTemplate(Modpack modpack, int[] modIds, int[] modpackIds, string modSettings)
        {
            Uid = globalUid;
            globalUid++;

            Modpack = modpack;
            Name = modpack.Name;
            ModIds = modIds;
            ModpackIds = modpackIds;
            ModSettings = modSettings;
        }



        public bool Equals(ModpackExportTemplate other)
        {
            if (other == null) return false;

            return this.Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            ModpackExportTemplate other = obj as ModpackExportTemplate;
            if (other == null) return false;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(ModpackExportTemplate first, ModpackExportTemplate second)
        {
            if (ReferenceEquals(first, null)) return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(ModpackExportTemplate first, ModpackExportTemplate second)
        {
            if (ReferenceEquals(first, null)) return !ReferenceEquals(second, null);

            return !first.Equals(second);
        }
    }
}
