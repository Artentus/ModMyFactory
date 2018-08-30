using System;
using System.ComponentModel;
using ModMyFactory.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ModExportTemplate : IEquatable<ModExportTemplate>
    {
        //---------------------------------------------------- Deprecated ------------------------------------------------------------

        public static ModExportTemplate FromMod(Mod mod, bool includeVersionInfo)
        {
            return new ModExportTemplate(mod.Name, includeVersionInfo ? mod.Version : null);
        }

        private ModExportTemplate(string name, Version version)
            : this(-1, name, ExportMode.Version1, version, null)
        { }

        //----------------------------------------------------------------------------------------------------------------------------




        private static int globalUid = 0;

        public static void ResetUid()
        {
            globalUid = 0;
        }



        [JsonIgnore]
        public Mod Mod { get; set; }

        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Uid { get; }

        public string Name { get; }

        [DefaultValue(ExportMode.Version1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ExportMode ExportMode { get; }

        [DefaultValue(null)]
        [JsonConverter(typeof(VersionConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Version Version { get; }

        [DefaultValue(null)]
        [JsonConverter(typeof(VersionConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Version FactorioVersion { get; }

        [JsonIgnore]
        public bool Included => ExportMode.HasFlag(ExportMode.Included);

        [JsonIgnore]
        public bool DownloadNewer => ExportMode.HasFlag(ExportMode.DownloadNewer);

        [JsonIgnore]
        public ExportMode MaskedExportMode => ExportMode & ExportMode.Mask;



        [JsonConstructor]
        private ModExportTemplate(int uid, string name, ExportMode exportMode, Version version, Version factorioVersion)
        {
            Uid = uid;
            Name = name;
            ExportMode = exportMode;
            Version = version;
            FactorioVersion = factorioVersion;
        }

        public ModExportTemplate(Mod mod, ExportMode exportMode)
        {
            Uid = globalUid;
            globalUid++;

            Mod = mod;
            Name = mod.Name;
            ExportMode = exportMode;

            var modeValue = exportMode & ExportMode.Mask;
            switch (modeValue)
            {
                case ExportMode.SpecificVersion:
                    Version = mod.Version;
                    FactorioVersion = null;
                    break;
                case ExportMode.FactorioVersion:
                    Version = null;
                    FactorioVersion = mod.FactorioVersion;
                    break;
                default:
                    Version = null;
                    FactorioVersion = null;
                    break;
            }
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
