using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        //--------------------------------------------------------------------------------------------------- Deprecated -----------------------------------------------------------------------------------------------
        
        private async Task<Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>>> GetModsToDownload(ExportTemplate template, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var toDownload = new List<ModRelease>();
            var conflicting = new List<Tuple<Mod, ModExportTemplate>>();

            int modCount = template.Mods.Length;
            int counter = 0;
            foreach (var modTemplate in template.Mods)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                progress.Report(new Tuple<double, string>((double)counter / modCount, modTemplate.Name));
                counter++;

                ExtendedModInfo modInfo = null;
                try
                {
                    modInfo = await ModWebsite.GetExtendedInfoAsync(modTemplate.Name);
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.ProtocolError) throw;
                }

                if (modInfo != null)
                {
                    if (template.IncludesVersionInfo)
                    {
                        if (!Mods.Contains(modTemplate.Name, modTemplate.Version))
                        {
                            Mod[] mods = Mods.Find(modTemplate.Name);

                            ModRelease release = modInfo.Releases.FirstOrDefault(r => r.Version == modTemplate.Version);

                            if (release != null)
                            {
                                if (mods.Length == 0)
                                {
                                    toDownload.Add(release);
                                }
                                else
                                {
                                    if ((App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion) &&
                                        mods.All(mod => mod.FactorioVersion != release.InfoFile.FactorioVersion))
                                    {
                                        toDownload.Add(release);
                                    }
                                    else
                                    {
                                        conflicting.Add(new Tuple<Mod, ModExportTemplate>(mods[0], modTemplate));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Mod[] mods = Mods.Find(modTemplate.Name);

                        if (mods.Length == 0)
                        {
                            ModRelease newestRelease = GetNewestModRelease(modInfo);
                            toDownload.Add(newestRelease);
                        }
                        else
                        {
                            ModRelease newestRelease = GetNewestModRelease(modInfo);

                            if (!Mods.Contains(modTemplate.Name, newestRelease.Version))
                            {
                                if ((App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion) &&
                                    mods.All(mod => mod.FactorioVersion != newestRelease.InfoFile.FactorioVersion))
                                {
                                    toDownload.Add(newestRelease);
                                }
                                else
                                {
                                    conflicting.Add(new Tuple<Mod, ModExportTemplate>(mods[0], modTemplate));
                                }
                            }
                        }
                    }
                }
            }

            progress.Report(new Tuple<double, string>(1, string.Empty));

            return new Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>>(toDownload, conflicting);
        }

        private async Task DownloadModAsyncInner(ModRelease modRelease, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            Mod mod = await ModWebsite.DownloadReleaseAsync(modRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken, Mods, Modpacks);
            if (!cancellationToken.IsCancellationRequested && (mod != null)) Mods.Add(mod);
        }

        private async Task DownloadModsAsyncInner(List<ModRelease> modReleases, string token, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            int modCount = modReleases.Count;
            double baseProgressValue = 0;
            foreach (var release in modReleases)
            {
                if (cancellationToken.IsCancellationRequested) return;

                double modProgressValue = 0;
                var modProgress = new Progress<double>(value =>
                {
                    modProgressValue = value / modCount;
                    progress.Report(new Tuple<double, string>(baseProgressValue + modProgressValue, release.FileName));
                });

                try
                {
                    await DownloadModAsyncInner(release, token, modProgress, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                        App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                baseProgressValue += modProgressValue;
            }
        }

        private async Task DownloadModsAsync(List<ModRelease> modReleases)
        {
            string token;
            if (GlobalCredentials.Instance.LogIn(Window, out token))
            {
                var progressWindow = new ProgressWindow() { Owner = Window };
                var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

                var progress = new Progress<Tuple<double, string>>(info =>
                {
                    progressViewModel.Progress = info.Item1;
                    progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), info.Item2);
                });

                var cancellationSource = new CancellationTokenSource();
                progressViewModel.CanCancel = true;
                progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                Task updateTask = DownloadModsAsyncInner(modReleases, token, progress, cancellationSource.Token);
                Task closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                progressWindow.ShowDialog();

                await updateTask;
                await closeWindowTask;
            }
        }

        private async Task ImportModpackFile(ExportTemplate template)
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

            var progress = new Progress<Tuple<double, string>>(info =>
            {
                progressViewModel.Progress = info.Item1;
                progressViewModel.ProgressDescription = info.Item2;
            });

            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CanCancel = true;
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            Tuple<List<ModRelease>, List<Tuple<Mod, ModExportTemplate>>> toDownloadResult;
            try
            {
                Task closeWindowTask = null;
                try
                {
                    var getModsTask = GetModsToDownload(template, progress, cancellationSource.Token);

                    closeWindowTask = getModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    toDownloadResult = await getModsTask;
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
            List<ModRelease> toDownload = toDownloadResult.Item1;
            List<Tuple<Mod, ModExportTemplate>> conflicting = toDownloadResult.Item2;

            if (conflicting.Count > 0)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("HasConflicts", MessageType.Warning) + "\n"
                    + string.Join("\n", conflicting.Select(conflict => $"{conflict.Item1.Name} ({conflict.Item1.Version}) <-> {conflict.Item2.Name}"
                    + (template.IncludesVersionInfo ? $" ({conflict.Item2.Version})" : " (latest)"))),
                    App.Instance.GetLocalizedMessageTitle("HasConflicts", MessageType.Warning),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            try
            {
                if (toDownload.Count > 0)
                    await DownloadModsAsync(toDownload);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var modpackTemplate in template.Modpacks)
            {
                var existingModpack = Modpacks.FirstOrDefault(item => item.Name == modpackTemplate.Name);

                if (existingModpack == null)
                {
                    Modpack modpack = new Modpack(modpackTemplate.Name, Modpacks);
                    modpack.ParentView = ModpacksView;

                    foreach (var modTemplate in modpackTemplate.Mods)
                    {
                        if (template.IncludesVersionInfo)
                        {
                            Mod mod = Mods.Find(modTemplate.Name, modTemplate.Version);
                            if (mod != null) modpack.Mods.Add(new ModReference(mod, modpack));
                        }
                        else
                        {
                            Mod mod = Mods.Find(modTemplate.Name).MaxBy(item => item.Version, new VersionComparer());
                            if (mod != null) modpack.Mods.Add(new ModReference(mod, modpack));
                        }
                    }

                    Modpacks.Add(modpack);
                }
                else
                {
                    foreach (var modTemplate in modpackTemplate.Mods)
                    {
                        if (template.IncludesVersionInfo)
                        {
                            Mod mod = Mods.Find(modTemplate.Name, modTemplate.Version);
                            if ((mod != null) && !existingModpack.Contains(mod)) existingModpack.Mods.Add(new ModReference(mod, existingModpack));
                        }
                        else
                        {
                            Mod mod = Mods.Find(modTemplate.Name).MaxBy(item => item.Version, new VersionComparer());
                            if ((mod != null) && !existingModpack.Contains(mod)) existingModpack.Mods.Add(new ModReference(mod, existingModpack));
                        }
                    }
                }
            }
            foreach (var modpackTemplate in template.Modpacks)
            {
                var existingModpack = Modpacks.FirstOrDefault(item => item.Name == modpackTemplate.Name);

                if (existingModpack != null)
                {
                    foreach (var innerTemplate in modpackTemplate.Modpacks)
                    {
                        Modpack modpack = Modpacks.FirstOrDefault(item => item.Name == innerTemplate);
                        if ((modpack != null) && !existingModpack.Contains(modpack)) existingModpack.Mods.Add(new ModpackReference(modpack, existingModpack));
                    }
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------




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

        private bool ModAlreadyInstalled(string name, Version factorioVersion, out Mod installedMod)
        {
            installedMod = Mods.FindByFactorioVersion(name, factorioVersion);
            return installedMod != null;
        }

        private string GetNameWithoutUid(string name)
        {
            int index = name.IndexOf('+');
            return name.Substring(index + 1);
        }

        private async Task AddZippedMod(FileInfo file, string name, Version version, Version factorioVersion, ModExportTemplate modTemplate)
        {
            var destDir = App.Instance.Settings.GetModDirectory(factorioVersion);
            await file.MoveToAsync(Path.Combine(destDir.FullName, GetNameWithoutUid(file.Name)));

            var mod = new ZippedMod(name, version, factorioVersion, file, Mods, Modpacks);
            modTemplate.Mod = mod;
            Mods.Add(mod);
        }

        private async Task AddExtractedMod(DirectoryInfo directory, string name, Version version, Version factorioVersion, ModExportTemplate modTemplate)
        {
            var destDir = App.Instance.Settings.GetModDirectory(factorioVersion);
            var newDir = new DirectoryInfo(Path.Combine(destDir.FullName, GetNameWithoutUid(directory.Name)));
            await directory.MoveToAsync(newDir.FullName);

            var mod = new ExtractedMod(name, version, factorioVersion, newDir, Mods, Modpacks);
            modTemplate.Mod = mod;
            Mods.Add(mod);
        }

        private async Task AddMod(ModExportTemplate modTemplate, DirectoryInfo fileLocation)
        {
            var fsInfo = GetIncludedFileOrDirectory(modTemplate, fileLocation);

            FileInfo file = fsInfo as FileInfo;
            if (file != null)
            {
                string name;
                Version version;
                Version factorioVersion;
                if (Mod.ArchiveFileValid(file, out factorioVersion, out name, out version, true))
                {
                    Mod installedMod;
                    if (ModAlreadyInstalled(name, factorioVersion, out installedMod))
                    {
                        if (installedMod.Version >= version)
                        {
                            modTemplate.Mod = installedMod;
                        }
                        else
                        {
                            await AddZippedMod(file, name, version, factorioVersion, modTemplate);

                            if (!(App.Instance.Settings.KeepOldModVersions || (App.Instance.Settings.KeepOldExtractedModVersions && (installedMod is ExtractedMod))))
                                installedMod.DeleteFilesystemObjects();
                        }
                    }
                    else
                    {
                        await AddZippedMod(file, name, version, factorioVersion, modTemplate);
                    }

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
                string name;
                Version version;
                Version factorioVersion;
                if (Mod.DirectoryValid(directory, out factorioVersion, out name, out version, true))
                {
                    Mod installedMod;
                    if (ModAlreadyInstalled(name, factorioVersion, out installedMod))
                    {
                        if (installedMod.Version >= version)
                        {
                            modTemplate.Mod = installedMod;
                        }
                        else
                        {
                            await AddExtractedMod(directory, name, version, factorioVersion, modTemplate);

                            if (!(App.Instance.Settings.KeepOldModVersions || (App.Instance.Settings.KeepOldExtractedModVersions && (installedMod is ExtractedMod))))
                                installedMod.DeleteFilesystemObjects();
                        }
                    }
                    else
                    {
                        await AddExtractedMod(directory, name, version, factorioVersion, modTemplate);
                    }
                    
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Invalid mod file.");
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
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

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
                await Task.Run(() => ZipFile.ExtractToDirectory(archiveFile.FullName, tempDir.FullName));
            }
            catch
            {
                ShowInvalidModpackError();
            }

            var packFile = new FileInfo(Path.Combine(tempDir.FullName, "pack.json"));
            if (!packFile.Exists) ShowInvalidModpackError();
            
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
                            //Fall back to old import code
                            await ImportModpackFile(template);
                        }
                    }
                }

                ModpackTemplateList.Instance.Update(Modpacks);
                ModpackTemplateList.Instance.Save();
                Refresh();
            }
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
