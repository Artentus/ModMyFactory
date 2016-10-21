using Newtonsoft.Json;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ExportTemplate
    {
        public bool IncludesVersionInfo { get; }

        public ModExportTemplate[] Mods { get; }

        public ModpackExportTemplate[] Modpacks { get; }

        [JsonConstructor]
        public ExportTemplate(bool includesVersionInfo, ModExportTemplate[] mods, ModpackExportTemplate[] modpacks)
        {
            IncludesVersionInfo = includesVersionInfo;
            Mods = mods;
            Modpacks = modpacks;
        }
    }
}
