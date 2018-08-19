using System.Collections.Generic;
using System.IO;
using ModMyFactory.Helpers;
using ModMyFactory.Models;

namespace ModMyFactory.Export
{
    static class ModpackExport
    {
        //--------------------------------------------------------------- Deprecated ------------------------------------------------------------------------------------

        private static void AddModpacksRecursive(Modpack modpack, ICollection<ModpackExportTemplate> templateCollection, bool includeVersionInfo)
        {
            var template = ModpackExportTemplate.FromModpack(modpack, includeVersionInfo);
            if (!templateCollection.Contains(template)) templateCollection.Add(template);

            foreach (var reference in modpack.Mods)
            {
                ModpackReference modpackReference = reference as ModpackReference;
                if (modpackReference != null)
                {
                    Modpack subModpack = modpackReference.Modpack;
                    AddModpacksRecursive(subModpack, templateCollection, includeVersionInfo);
                }
            }
        }

        public static ExportTemplate CreateTemplate(IEnumerable<Modpack> modpacks, bool includeVersionInfo)
        {
            var modTemplates = new List<ModExportTemplate>();
            var modpackTemplates = new List<ModpackExportTemplate>();

            foreach (var modpack in modpacks)
                AddModpacksRecursive(modpack, modpackTemplates, includeVersionInfo);

            foreach (var template in modpackTemplates)
            {
                foreach (var mod in template.Mods)
                {
                    if (!modTemplates.Contains(mod)) modTemplates.Add(mod);
                }
            }

            return new ExportTemplate(includeVersionInfo, modTemplates.ToArray(), modpackTemplates.ToArray());
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------
        









        public static void ExportTemplate(ExportTemplate template, string filePath)
        {
            var file = new FileInfo(filePath);
            JsonHelper.Serialize(template, file);
        }

        public static ExportTemplate ImportTemplate(FileInfo file)
        {
            return JsonHelper.Deserialize<ExportTemplate>(file);
        }
    }
}
