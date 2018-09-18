using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ModMyFactory.ViewModels
{
    partial class MainViewModel
    {
        private class ModGrouping
        {
            public string Name { get; }

            public string FriendlyName => ModVersions.MaxBy(modVersion => modVersion.Version).FriendlyName;

            public List<Mod> ModVersions { get; }

            public ModGrouping(string name)
            {
                Name = name;
                ModVersions = new List<Mod>();
            }
        }

        private ICollection<ModGrouping> GetUniqueMods()
        {
            var dict = new Dictionary<string, ModGrouping>();

            foreach (var mod in Mods)
            {
                if (dict.ContainsKey(mod.Name))
                {
                    dict[mod.Name].ModVersions.Add(mod);
                }
                else
                {
                    var grouping = new ModGrouping(mod.Name);
                    grouping.ModVersions.Add(mod);
                    dict.Add(mod.Name, grouping);
                }
            }

            return dict.Values;
        }

        private ICollection<ModUpdateInfo> GetReleaseDownloadCandidates(ModGrouping grouping, ExtendedModInfo info)
        {
            var candidates = new List<ModRelease>();
            var result = new List<ModUpdateInfo>();

            var newestRelease = GetNewestModRelease(info);
            Mod newestModVersion = grouping.ModVersions.MaxBy(modVersion => modVersion.Version, new VersionComparer());
            if (newestRelease.Version > newestModVersion.Version)
            {
                bool exchange = (newestModVersion.FactorioVersion == newestRelease.InfoFile.FactorioVersion) || !App.Instance.Settings.DownloadIntermediateUpdates;
                bool keepOld = newestModVersion.AlwaysKeepOnUpdate()
                    || ((App.Instance.Settings.KeepOldModVersionsWhenNewFactorioVersion || App.Instance.Settings.DownloadIntermediateUpdates) && (newestModVersion.FactorioVersion != newestRelease.InfoFile.FactorioVersion));

                candidates.Add(newestRelease);
                result.Add(new ModUpdateInfo(newestModVersion, newestRelease, exchange, keepOld));
            }

            if (App.Instance.Settings.DownloadIntermediateUpdates)
            {
                foreach (var modVersion in grouping.ModVersions)
                {
                    var releaseList = info.Releases.Where(release => release.InfoFile.FactorioVersion == modVersion.FactorioVersion);
                    var newest = releaseList.MaxBy(release => release.Version, new VersionComparer());
                    var updateInfo = new ModUpdateInfo(modVersion, newest, true, modVersion.AlwaysKeepOnUpdate());

                    if ((newest.Version > modVersion.Version) && (!candidates.Contains(newest) && !Mods.Contains(modVersion.Name, newest.Version)))
                    {
                        candidates.Add(newest);
                        result.Add(updateInfo);
                    }
                }
            }

            return result;
        }

        private async Task<List<ModUpdateInfo>> GetModUpdatesAsync(IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var result = new List<ModUpdateInfo>();

            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {

            }
            else
            {
                int modCount = Mods.Count;
                int index = 0;
                foreach (var mod in Mods)
                {
                    progress.Report(new Tuple<double, string>((double)index / modCount, mod.FriendlyName));

                    var extendedInfo = await ModWebsite.GetExtendedInfoAsync(mod.Name);
                    if (extendedInfo.LatestRelease.Version > mod.Version)
                        result.Add(new ModUpdateInfo(mod, extendedInfo.LatestRelease, false));

                    index++;
                }
            }

            return result;
        }

        private async Task UpdateModAsyncInner(ModUpdateInfo modUpdate, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            FileInfo updateFile = await ModWebsite.UpdateReleaseAsync(modUpdate.NewestRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken);
            Mod mod = modUpdate.Mod;

            if (mod.ExtractUpdates)
            {
                DirectoryInfo updateDirectory = await Task.Run(() =>
                {
                    DirectoryInfo modsDirectory = App.Instance.Settings.GetModDirectory(modUpdate.NewestRelease.InfoFile.FactorioVersion);
                    ZipFile.ExtractToDirectory(updateFile.FullName, modsDirectory.FullName);
                    updateFile.Delete();

                    return new DirectoryInfo(Path.Combine(modsDirectory.FullName, updateFile.NameWithoutExtension()));
                });
            }

            if (!modUpdate.KeepOld) mod.DeleteFilesystemObjects();
            if (!App.Instance.Settings.DownloadIntermediateUpdates) mod.PrepareUpdate(modUpdate.UpdateFactorioVersion);
        }

        private async Task UpdateModsAsyncInner(List<ModUpdateInfo> modUpdates, string token, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            int modCount = modUpdates.Count(item => item.IsSelected);
            double baseProgressValue = 0;
            foreach (var modUpdate in modUpdates)
            {
                if (cancellationToken.IsCancellationRequested) return;

                if (modUpdate.IsSelected)
                {
                    double modProgressValue = 0;
                    var modProgress = new Progress<double>(value =>
                    {
                        modProgressValue = value / modCount;
                        progress.Report(new Tuple<double, string>(baseProgressValue + modProgressValue, modUpdate.Title));
                    });

                    try
                    {
                        await UpdateModAsyncInner(modUpdate, token, modProgress, cancellationToken);
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

            ModpackTemplateList.Instance.Update(Modpacks);
            ModpackTemplateList.Instance.Save();

            Refresh();
        }

        private async Task UpdateMods()
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("SearchingForUpdatesAction");

            var progress = new Progress<Tuple<double, string>>(info =>
            {
                progressViewModel.Progress = info.Item1;
                progressViewModel.ProgressDescription = info.Item2;
            });

            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CanCancel = true;
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            List<ModUpdateInfo> modUpdates;
            try
            {
                Task closeWindowTask = null;
                try
                {
                    Task<List<ModUpdateInfo>> searchForUpdatesTask = GetModUpdatesAsync(progress, cancellationSource.Token);

                    closeWindowTask = searchForUpdatesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    modUpdates = await searchForUpdatesTask;
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

            if (!cancellationSource.IsCancellationRequested)
            {
                if (modUpdates.Count > 0)
                {
                    var updateWindow = new ModUpdateWindow() { Owner = Window };
                    var updateViewModel = (ModUpdateViewModel)updateWindow.ViewModel;
                    updateViewModel.ModsToUpdate = modUpdates;
                    bool? result = updateWindow.ShowDialog();

                    if (result.HasValue && result.Value)
                    {
                        string token;
                        if (GlobalCredentials.Instance.LogIn(Window, out token))
                        {
                            progressWindow = new ProgressWindow() { Owner = Window };
                            progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("UpdatingModsAction");

                            progress = new Progress<Tuple<double, string>>(info =>
                            {
                                progressViewModel.Progress = info.Item1;
                                progressViewModel.ProgressDescription = info.Item2;
                            });

                            cancellationSource = new CancellationTokenSource();
                            progressViewModel.CanCancel = true;
                            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                            Task updateTask = UpdateModsAsyncInner(modUpdates, token, progress, cancellationSource.Token);

                            Task closeWindowTask = updateTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                            progressWindow.ShowDialog();

                            await updateTask;
                            await closeWindowTask;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Window,
                        App.Instance.GetLocalizedMessage("NoModUpdates", MessageType.Information),
                        App.Instance.GetLocalizedMessageTitle("NoModUpdates", MessageType.Information),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
