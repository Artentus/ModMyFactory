using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.Export;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;

namespace ModMyFactory.ViewModels
{
    partial class MainViewModel
    {
        private async Task<ExtendedModInfo> GetModInfo(ModExportTemplate modTemplate)
        {
            ExtendedModInfo info = null;
            try
            {
                info = await ModWebsite.GetExtendedInfoAsync(modTemplate.Name);
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError) throw;
            }

            return info;
        }

        private FileSystemInfo GetIncludedFileOrDirectory(ModExportTemplate modTemplate, DirectoryInfo fileLocation)
        {
            return fileLocation.EnumerateFileSystemInfos($"{modTemplate.Uid}+*").FirstOrDefault();
        }

        private async Task DownloadModRelease(ModExportTemplate modTemplate, ModRelease release, DirectoryInfo fileLocation, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var installedMod = Mods.FindByFactorioVersion(modTemplate.Name, release.InfoFile.FactorioVersion);
            if ((installedMod != null) && (installedMod.Version >= release.Version))
            {
                modTemplate.Mod = installedMod;
                return;
            }

            string token;
            GlobalCredentials.Instance.LogIn(Window, out token);

            string fileName = Path.Combine(fileLocation.FullName, $"{modTemplate.Uid}+{release.FileName}");
            await ModWebsite.DownloadReleaseToFileAsync(release, GlobalCredentials.Instance.Username, token, fileName, progress, cancellationToken);
        }

        private Version GetVersionFromFile(FileSystemInfo file)
        {
            string[] parts = file.NameWithoutExtension().Split('_');
            return Version.Parse(parts[parts.Length - 1]);
        }

        private async Task DownloadIncludedMod(ModExportTemplate modTemplate, DirectoryInfo fileLocation, ExtendedModInfo info, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var file = GetIncludedFileOrDirectory(modTemplate, fileLocation);
            if ((file == null) || !file.Exists) // File should exist, download if it doesn't
            {
                await DownloadExcludedMod(modTemplate, fileLocation, info, progress, cancellationToken);
                return;
            }

            ModRelease targetRelease = null;
            switch (modTemplate.MaskedExportMode)
            {
                case ExportMode.NewestVersion:
                    targetRelease = info.Releases.MaxBy(release => release.Version);
                    break;
                case ExportMode.SpecificVersion:
                    targetRelease = info.Releases.FirstOrDefault(release => release.Version == modTemplate.Version);
                    break;
                case ExportMode.FactorioVersion:
                    targetRelease = info.Releases.Where(release => release.InfoFile.FactorioVersion == modTemplate.FactorioVersion).MaxBy(release => release.Version);
                    break;
            }
            
            if (targetRelease != null)
            {
                var fileVersion = GetVersionFromFile(file);
                if (fileVersion != targetRelease.Version)
                {
                    file.Delete();
                    await DownloadModRelease(modTemplate, targetRelease, fileLocation, progress, cancellationToken);
                }
            }
        }

        private async Task DownloadExcludedMod(ModExportTemplate modTemplate, DirectoryInfo fileLocation, ExtendedModInfo info, IProgress<double> progress, CancellationToken cancellationToken)
        {
            switch (modTemplate.MaskedExportMode)
            {
                case ExportMode.NewestVersion:
                    var newestRelease = info.Releases.MaxBy(release => release.Version);
                    if (newestRelease != null) await DownloadModRelease(modTemplate, newestRelease, fileLocation, progress, cancellationToken);
                    break;
                case ExportMode.SpecificVersion:
                    var specificRelease = info.Releases.FirstOrDefault(release => release.Version == modTemplate.Version);
                    if (specificRelease != null) await DownloadModRelease(modTemplate, specificRelease, fileLocation, progress, cancellationToken);
                    break;
                case ExportMode.FactorioVersion:
                    var factorioRelease = info.Releases.Where(release => release.InfoFile.FactorioVersion == modTemplate.FactorioVersion).MaxBy(release => release.Version);
                    if (factorioRelease != null) await DownloadModRelease(modTemplate, factorioRelease, fileLocation, progress, cancellationToken);
                    break;
            }
        }

        private async Task DownloadIfMissing(ModExportTemplate modTemplate, DirectoryInfo fileLocation, IProgress<double> progress, CancellationToken cancellationToken)
        {
            progress.Report(0);

            var info = await GetModInfo(modTemplate);
            if (info != null)
            {
                if (modTemplate.Included)
                {
                    if (modTemplate.DownloadNewer)
                    {
                        await DownloadIncludedMod(modTemplate, fileLocation, info, progress, cancellationToken);
                    }
                    else
                    {
                        var file = GetIncludedFileOrDirectory(modTemplate, fileLocation);
                        if ((file == null) || !file.Exists) await DownloadExcludedMod(modTemplate, fileLocation, info, progress, cancellationToken); // File should exist, download if it doesn't
                    }
                }
                else
                {
                    var file = GetIncludedFileOrDirectory(modTemplate, fileLocation);
                    if ((file != null) && file.Exists) file.DeleteRecursive(); // File should not exist, delete if it does

                    await DownloadExcludedMod(modTemplate, fileLocation, info, progress, cancellationToken);
                }
            }

            progress.Report(1);
        }
        
        private async Task AddMod(ModExportTemplate modTemplate, DirectoryInfo fileLocation)
        {
            var fsInfo = GetIncludedFileOrDirectory(modTemplate, fileLocation);

            FileInfo file = fsInfo as FileInfo;
            if (file != null)
            {
                if (ModFile.TryLoadFromFile(file, out var modFile, true))
                {
                    Mod mod = await Mod.Add(modFile, Mods, Modpacks, false, true);
                    modTemplate.Mod = mod;
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Invalid mod file.");
                }
            }

            DirectoryInfo directory = fsInfo as DirectoryInfo;
            if (directory != null)
            {
                if (ModFile.TryLoadFromDirectory(directory, out var modFile, true))
                {
                    Mod mod = await Mod.Add(modFile, Mods, Modpacks, false, true);
                    modTemplate.Mod = mod;
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Invalid mod directory.");
                }
            }
        }

        private async Task DownloadImportedMods(ExportTemplate template, DirectoryInfo fileLocation, IProgress<double> progress, CancellationToken cancellationToken)
        {
            int progressIndex = 0;
            int progressCount = template.Mods.Length;
            foreach (var modTemplate in template.Mods)
            {
                if (cancellationToken.IsCancellationRequested) break;
                var subProgress = new Progress<double>(p => progress.Report((progressIndex + p) / progressCount));

                await DownloadIfMissing(modTemplate, fileLocation, subProgress, cancellationToken);

                progressIndex++;
            }
        }

        private Mod GetModFromUid(ExportTemplate template, int uid)
        {
            return template.Mods.FirstOrDefault(modTemplate => modTemplate.Uid == uid)?.Mod;
        }

        private Modpack GetModpackFromUid(ExportTemplate template, int uid)
        {
            return template.Modpacks.FirstOrDefault(modpackTemplate => modpackTemplate.Uid == uid)?.Modpack;
        }

        private async Task ImportModpackFileV2(ExportTemplate template, DirectoryInfo fileLocation)
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("ImportingAction");
            progressViewModel.ProgressDescription = App.Instance.GetLocalizedResourceString("ImportingDownloadingDescription");

            var progress = new Progress<double>(p => progressViewModel.Progress = p);

            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CanCancel = true;
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();
            
            try
            {
                Task closeWindowTask = null;
                try
                {
                    Task downloadTask = DownloadImportedMods(template, fileLocation, progress, cancellationSource.Token);

                    closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    await downloadTask;
                }
                finally
                {
                    if (closeWindowTask != null) await closeWindowTask;
                }
            }
            catch (WebException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!cancellationSource.IsCancellationRequested)
            {
                foreach (var modTemplate in template.Mods)
                {
                    if (modTemplate.Mod == null)
                        await AddMod(modTemplate, fileLocation);
                }

                foreach (var modpackTemplate in template.Modpacks)
                {
                    var modpack = new Modpack(modpackTemplate.Name, Modpacks);
                    modpacks.Add(modpack);
                    modpackTemplate.Modpack = modpack;
                    
                    foreach (var modId in modpackTemplate.ModIds)
                    {
                        Mod mod = GetModFromUid(template, modId);
                        if (mod != null) modpack.Mods.Add(new ModReference(mod, modpack));
                    }

                    foreach (var modpackId in modpackTemplate.ModpackIds)
                    {
                        Modpack subModpack = GetModpackFromUid(template, modpackId);
                        if (subModpack != null) modpack.Mods.Add(new ModpackReference(subModpack, modpack));
                    }
                }
            }
        }

        private void ShowInvalidModpackError()
        {
            MessageBox.Show(Window,
                App.Instance.GetLocalizedMessage("InvalidModpack", MessageType.Error),
                App.Instance.GetLocalizedMessageTitle("InvalidModpack", MessageType.Error),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async Task ImportModpackArchive(FileInfo archiveFile)
        {
            var tempDir = new DirectoryInfo(App.Instance.TempPath);
            if (!tempDir.Exists) tempDir.Create();

            try
            {
                try
                {
                    await Task.Run(() => ZipFile.ExtractToDirectory(archiveFile.FullName, tempDir.FullName));
                }
                catch
                {
                    ShowInvalidModpackError();
                    return;
                }

                var packFile = new FileInfo(Path.Combine(tempDir.FullName, "pack.json"));
                if (!packFile.Exists)
                {
                    ShowInvalidModpackError();
                    return;
                }

                try
                {
                    var template = ModpackExport.ImportTemplate(packFile);
                    if (template.Version != 2)
                    {
                        ShowInvalidModpackError();
                        return;
                    }

                    await ImportModpackFileV2(template, tempDir);
                }
                catch (JsonSerializationException)
                {
                    ShowInvalidModpackError();
                    return;
                }
            }
            finally
            {
                tempDir.Delete(true);
            }
        }

        private async Task ImportModpacksInner(IEnumerable<FileInfo> modpackFiles)
        {
            foreach (FileInfo file in modpackFiles)
            {
                if (file.Exists)
                {
                    if (file.Extension == ".fmpa")
                    {
                        await ImportModpackArchive(file);
                    }
                    else
                    {
                        ExportTemplate template;
                        try
                        {
                            template = ModpackExport.ImportTemplate(file);
                        }
                        catch (JsonSerializationException)
                        {
                            ShowInvalidModpackError();
                            continue;
                        }

                        if (template.Version == 2)
                        {
                            var tempDir = new DirectoryInfo(App.Instance.TempPath);
                            if (!tempDir.Exists) tempDir.Create();

                            try
                            {
                                await ImportModpackFileV2(template, tempDir);
                            }
                            finally
                            {
                                tempDir.Delete(true);
                            }
                        }
                        else
                        {
                            MessageBox.Show(Window,
                                App.Instance.GetLocalizedMessage("FMPv1", MessageType.Information),
                                App.Instance.GetLocalizedMessageTitle("FMPv1", MessageType.Information),
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

                ModpackTemplateList.Instance.Update(Modpacks);
                ModpackTemplateList.Instance.Save();
            }

            Mods.EvaluateDependencies();
        }

        private async Task ImportModpacks()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = App.Instance.GetLocalizedResourceString("AllCompatibleDescription") + @" (*.fmp;*.fmpa)|*.fmp;*.fmpa|"
                            + App.Instance.GetLocalizedResourceString("FmpDescription") + @" (*.fmp)|*.fmp|"
                            + App.Instance.GetLocalizedResourceString("FmpaDescription") + @" (*.fmpa)|*.fmpa";
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog(Window);
            if (result.HasValue && result.Value)
            {
                var fileList = new List<FileInfo>();
                foreach (var fileName in dialog.FileNames)
                {
                    var file = new FileInfo(fileName);
                    fileList.Add(file);
                }

                if (fileList.Count > 0)
                    await ImportModpacksInner(fileList);
            }
        }
    }
}
