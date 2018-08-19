using System.ComponentModel;
using Newtonsoft.Json;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ExportTemplate
    {
        //---------------------------------------------------- Deprecated ------------------------------------------------------------

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IncludesVersionInfo { get; }

        public ExportTemplate(bool includesVersionInfo, ModExportTemplate[] mods, ModpackExportTemplate[] modpacks)
            : this(includesVersionInfo, mods, modpacks, 1)
        { }

        //----------------------------------------------------------------------------------------------------------------------------




        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Version { get; }
        
        public ModExportTemplate[] Mods { get; }

        public ModpackExportTemplate[] Modpacks { get; }

        [JsonConstructor]
        private ExportTemplate(bool includesVersionInfo, ModExportTemplate[] mods, ModpackExportTemplate[] modpacks, int version)
        {
            IncludesVersionInfo = includesVersionInfo;
            Mods = mods;
            Modpacks = modpacks;
            Version = version;
        }

        public ExportTemplate(ModExportTemplate[] mods, ModpackExportTemplate[] modpacks)
            : this(false, mods, modpacks, 2)
        { }
    }
}
