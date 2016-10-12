using Newtonsoft.Json;

namespace ModMyFactory.Export
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ExportTemplate
    {
        public ModExportTemplate[] Mods { get; }

        public ModpackExportTemplate[] Modpacks { get; }

        [JsonConstructor]
        public ExportTemplate(ModExportTemplate[] mods, ModpackExportTemplate[] modpacks)
        {
            Mods = mods;
            Modpacks = modpacks;
        }
    }
}
