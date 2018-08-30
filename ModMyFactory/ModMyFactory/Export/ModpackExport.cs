using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private static bool ModTemplatesEqual(ModTemplate template, ModExportTemplate exportTemplate, bool downloadNewer)
        {
            if (template.Mod == exportTemplate.Mod)
            {
                //Same mod, if options match templates are equal
                bool includeEquals = (template.Include == exportTemplate.Included);
                bool exportModeEquals = (template.ExportMode == exportTemplate.MaskedExportMode);

                return (includeEquals || downloadNewer) && exportModeEquals;
            }
            else if (template.Mod.Name == exportTemplate.Name)
            {
                if (template.Mod.Version == exportTemplate.Version)
                {
                    //Different objects but same mod, if options match templates are equal (this should actually never happen)
                    bool includeEquals = (template.Include == exportTemplate.Included);
                    bool exportModeEquals = (template.ExportMode == exportTemplate.MaskedExportMode);

                    return (includeEquals || downloadNewer) && exportModeEquals;
                }
                else
                {
                    //Different versions of same mod
                    if (downloadNewer || (!template.Include && !exportTemplate.Included))
                    {
                        if (template.UseNewestVersion && ((exportTemplate.ExportMode & ExportMode.Mask) == ExportMode.NewestVersion))
                        {
                            //Both use newest version, currently installed version doesn't matter, templates are equal
                            return true;
                        }
                        else if (template.UseFactorioVersion && ((exportTemplate.ExportMode & ExportMode.Mask) == ExportMode.FactorioVersion))
                        {
                            //Both use Factorio version, templates are equal if Factorio versions are equal
                            return template.Mod.FactorioVersion == exportTemplate.FactorioVersion;
                        }
                    }
                }
            }
            
            return false;
        }

        private static async Task<ModExportTemplate> AddUniqueModTemplate(ModTemplate template, List<ModExportTemplate> uniqueTemplates)
        {
            bool downloadNewer = template.Include && (template.UseNewestVersion || template.UseFactorioVersion);

            foreach (var exportTemplate in uniqueTemplates)
            {
                if (ModTemplatesEqual(template, exportTemplate, downloadNewer))
                    return exportTemplate;
            }

            var exportMode = template.ExportMode;
            if (template.Include) exportMode = exportMode | ExportMode.Included;
            if (downloadNewer) exportMode = exportMode | ExportMode.DownloadNewer;
            var newExportTemplate = new ModExportTemplate(template.Mod, exportMode);

            if (template.Include)
            {
                var tempDir = new DirectoryInfo(App.Instance.TempPath);
                if (!tempDir.Exists) tempDir.Create();

                var zippedMod = newExportTemplate.Mod as ZippedMod;
                if (zippedMod != null)
                {
                    var tempFile = new FileInfo(Path.Combine(tempDir.FullName, $"{newExportTemplate.Uid}+{zippedMod.File.Name}"));
                    await Task.Run(() => zippedMod.File.CopyTo(tempFile.FullName, true));
                }
                else
                {
                    var extractedMod = (ExtractedMod)newExportTemplate.Mod;
                    string tempName = Path.Combine(tempDir.FullName, extractedMod.Directory.Name);
                    await extractedMod.Directory.CopyToAsync(tempName);
                }
            }

            uniqueTemplates.Add(newExportTemplate);
            return newExportTemplate;
        }

        private static bool AllDependenciesContained(ModpackTemplate template, List<ModpackTemplate> list)
        {
            foreach (var dependency in template.ModpackTemplates.Select(inner => inner.OuterTemplate))
            {
                if (!list.Contains(dependency)) return false;
            }
            return true;
        }

        private static List<ModpackTemplate> ModpackTopologicalSort(IEnumerable<ModpackTemplate> modpacks)
        {
            var source = modpacks.ToList();
            var result = new List<ModpackTemplate>();

            while (source.Count > 0)
            {
                for (int i = source.Count - 1; i >= 0; i--)
                {
                    var template = source[i];
                    if (AllDependenciesContained(template, result))
                    {
                        result.Add(template);
                        source.RemoveAt(i);
                    }
                }
            }

            return result;
        }

        public static async Task<ExportTemplate> CreateTemplateV2(IEnumerable<ModpackTemplate> modpacks)
        {
            ModExportTemplate.ResetUid();
            ModpackExportTemplate.ResetUid();

            var modExportTemplates = new List<ModExportTemplate>();
            var modpackExportTemplates = new List<ModpackExportTemplate>();

            var sortedModpacks = ModpackTopologicalSort(modpacks);

            foreach (var modpack in sortedModpacks)
            {
                var modIds = new List<int>();
                var modpackIds = new List<int>();

                foreach (var mod in modpack.ModTemplates)
                {
                    var template = await AddUniqueModTemplate(mod, modExportTemplates);
                    modIds.Add(template.Uid);
                }

                foreach (var subModpack in modpack.ModpackTemplates)
                {
                    var exportTemplate = modpackExportTemplates.FirstOrDefault(et => et.Modpack == subModpack.Modpack);
                    if (exportTemplate != null) modpackIds.Add(exportTemplate.Uid);
                }

                var modpackExportTemplate = new ModpackExportTemplate(modpack.Modpack, modIds.ToArray(), modpackIds.ToArray());
                modpackExportTemplates.Add(modpackExportTemplate);
            }

            return new ExportTemplate(modExportTemplates.ToArray(), modpackExportTemplates.ToArray());
        }
    }
}
