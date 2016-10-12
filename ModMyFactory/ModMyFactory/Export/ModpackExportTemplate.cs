using System;
using System.Collections.Generic;
using ModMyFactory.Models;
using Newtonsoft.Json;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ModpackExportTemplate : IEquatable<ModpackExportTemplate>
    {
        public static ModpackExportTemplate FromModpack(Modpack modpack)
        {
            var mods = new List<ModExportTemplate>();
            var modpacks = new List<string>();

            foreach (var reference in modpack.Mods)
            {
                ModReference modReference = reference as ModReference;
                ModpackReference modpackReference = reference as ModpackReference;

                if (modReference != null)
                {
                    mods.Add(ModExportTemplate.FromMod(modReference.Mod));
                }
                else if (modpackReference != null)
                {
                    modpacks.Add(modpackReference.Modpack.Name);
                }
            }

            return new ModpackExportTemplate(modpack.Name, mods.ToArray(), modpacks.ToArray());
        }

        public string Name { get; }

        public ModExportTemplate[] Mods { get; }

        public string[] Modpacks { get; }

        [JsonConstructor]
        public ModpackExportTemplate(string name, ModExportTemplate[] mods, string[] modpacks)
        {
            Name = name;
            Mods = mods;
            Modpacks = modpacks;
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
