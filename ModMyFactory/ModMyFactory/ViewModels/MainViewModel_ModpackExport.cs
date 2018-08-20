using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ModMyFactory.Export;
using ModMyFactory.Models;
using ModMyFactory.Views;
using Ookii.Dialogs.Wpf;

namespace ModMyFactory.ViewModels
{
    partial class MainViewModel
    {
        private string BuildExportFilter(IEnumerable<ModpackTemplate> templates, out bool unpackedAllowed)
        {
            bool containsInclusions = templates.Where(modpackTemplate => modpackTemplate.Export).SelectMany(modpackTemplate => modpackTemplate.ModTemplates).Any(modTemplate => modTemplate.Include);

            string result = string.Empty;
            if (!containsInclusions) result = App.Instance.GetLocalizedResourceString("FmpDescription") + @" (*.fmp)|*.fmp|";
            result += App.Instance.GetLocalizedResourceString("FmpaDescription") + @" (*.fmpa)|*.fmpa";

            unpackedAllowed = !containsInclusions;
            return result;
        }

        private void ExportModpacks()
        {
            var exportWindow = new ModpackExportWindow() { Owner = Window };
            var exportViewModel = (ModpackExportViewModel)exportWindow.ViewModel;
            bool? result = exportWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var dialog = new VistaSaveFileDialog();
                bool unpackedAllowed;
                dialog.Filter = BuildExportFilter(exportViewModel.Modpacks, out unpackedAllowed);
                dialog.AddExtension = true;
                dialog.DefaultExt = unpackedAllowed ? ".fmp" : ".fmpa";
                result = dialog.ShowDialog(Window);
                if (result.HasValue && result.Value)
                {
                    if (dialog.FileName.EndsWith(".fmpa"))
                    {
                        var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "ModMyFactory"));
                        if (!tempDir.Exists) tempDir.Create();

                        var exportTemplate = ModpackExport.CreateTemplateV2(exportViewModel.Modpacks.Where(modpackTemplate => modpackTemplate.Export), exportViewModel.DownloadNewer);
                        ModpackExport.ExportTemplate(exportTemplate, Path.Combine(tempDir.FullName, "pack.json"));

                        ZipFile.CreateFromDirectory(tempDir.FullName, dialog.FileName);
                        tempDir.Delete(true);
                    }
                    else
                    {
                        var exportTemplate = ModpackExport.CreateTemplateV2(exportViewModel.Modpacks.Where(modpackTemplate => modpackTemplate.Export), exportViewModel.DownloadNewer);
                        ModpackExport.ExportTemplate(exportTemplate, dialog.FileName);
                    }
                }
            }
        }
    }
}
