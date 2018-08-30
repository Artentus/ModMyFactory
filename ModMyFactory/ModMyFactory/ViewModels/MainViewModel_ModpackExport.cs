using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
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

        private async Task ExportArchive(IEnumerable<ModpackTemplate> modpacks, string fileName)
        {
            var tempDir = new DirectoryInfo(App.Instance.TempPath);
            if (!tempDir.Exists) tempDir.Create();

            var exportTemplate = await ModpackExport.CreateTemplateV2(modpacks);
            ModpackExport.ExportTemplate(exportTemplate, Path.Combine(tempDir.FullName, "pack.json"));

            ZipFile.CreateFromDirectory(tempDir.FullName, fileName);
            tempDir.Delete(true);
        }

        private async Task ExportModpacks()
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
                        var progressWindow = new ProgressWindow() { Owner = Window };
                        var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                        progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("ExportingAction");

                        Task closeWindowTask = null;
                        try
                        {
                            var task = ExportArchive(exportViewModel.Modpacks.Where(modpackTemplate => modpackTemplate.Export), dialog.FileName);

                            closeWindowTask = task.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                            progressWindow.ShowDialog();
                        }
                        finally
                        {
                            if (closeWindowTask != null) await closeWindowTask;
                        }
                    }
                    else
                    {
                        var exportTemplate = await ModpackExport.CreateTemplateV2(exportViewModel.Modpacks.Where(modpackTemplate => modpackTemplate.Export));
                        ModpackExport.ExportTemplate(exportTemplate, dialog.FileName);
                    }
                }
            }
        }
    }
}
