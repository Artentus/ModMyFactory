using System;
using ModMyFactory.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ModExportTemplate : IEquatable<ModExportTemplate>
    {
        public static ModExportTemplate FromMod(Mod mod, bool includeVersionInfo)
        {
            return new ModExportTemplate(mod.Name, includeVersionInfo ? mod.Version : null);
        }

        public string Name { get; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; }

        [JsonConstructor]
        public ModExportTemplate(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public bool Equals(ModExportTemplate other)
        {
            if (other == null) return false;

            return (this.Name == other.Name) && (this.Version == other.Version);
        }

        public override bool Equals(object obj)
        {
            ModExportTemplate other = obj as ModExportTemplate;
            if (other == null) return false;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Version.GetHashCode();
        }

        public static bool operator ==(ModExportTemplate first, ModExportTemplate second)
        {
            if (ReferenceEquals(first, null)) return ReferenceEquals(second, null);

            return first.Equals(second);
        }

        public static bool operator !=(ModExportTemplate first, ModExportTemplate second)
        {
            if (ReferenceEquals(first, null)) return !ReferenceEquals(second, null);

            return !first.Equals(second);
        }
    }
}
