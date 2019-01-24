using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ModMyFactory.ViewModels
{
    partial class MainViewModel
    {
        private async Task AddInfoAsync(Dictionary<string, ExtendedModInfo> infos, string modName)
        {
            if (!infos.TryGetValue(modName, out var info))
            {
                try
                {
                    info = await ModWebsite.GetExtendedInfoAsync(modName);
                    infos.Add(modName, info);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if ((response != null) && (response.StatusCode == HttpStatusCode.NotFound)) return;
                    }

                    throw;
                }
            }
        }

        private void UpdaterAddMod(Dictionary<string, Dictionary<Version, List<Mod>>> mods, Mod mod)
        {
            if (!mods.TryGetValue(mod.Name, out var subDict))
            {
                subDict = new Dictionary<Version, List<Mod>>();
                mods.Add(mod.Name, subDict);
            }

            if (!subDict.TryGetValue(mod.FactorioVersion, out var list))
            {
                list = new List<Mod>();
                subDict.Add(mod.FactorioVersion, list);
            }

            list.Add(mod);
        }

        private void AddUpdateInfo(Dictionary<string, Dictionary<Version, ModUpdateInfo>> updateInfos, ModUpdateInfo updateInfo)
        {
            if (!updateInfos.TryGetValue(updateInfo.ModName, out var subDict))
            {
                subDict = new Dictionary<Version, ModUpdateInfo>();
                updateInfos.Add(updateInfo.ModName, subDict);
            }

            subDict[updateInfo.FactorioVersion] = updateInfo;
        }
        
        private async Task<List<ModUpdateInfo>> GetModUpdatesAsync(IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var mods = new Dictionary<string, Dictionary<Version, List<Mod>>>();
            var infos = new Dictionary<string, ExtendedModInfo>();

            int modCount = Mods.Count;
            int index = 0;
            foreach (var mod in Mods)
            {
                progress.Report(new Tuple<double, string>((double)index / modCount, mod.FriendlyName));
                if (cancellationToken.IsCancellationRequested) return null;

                UpdaterAddMod(mods, mod);
                await AddInfoAsync(infos, mod.Name);

                index++;
            }


            var updateInfos = new Dictionary<string, Dictionary<Version, ModUpdateInfo>>();
            foreach (var kvp in mods)
            {
                string modName = kvp.Key;
                if (infos.TryGetValue(modName, out var info))
                {
                    foreach (var group in kvp.Value)
                    {
                        var factorioVersion = group.Key;
                        var groupedMods = group.Value;

                        var latestRelease = info.GetLatestRelease(factorioVersion);
                        if (latestRelease != null)
                        {
                            var latestVersion = groupedMods.MaxBy(m => m.Version, new VersionComparer());
                            if (latestVersion.Version < latestRelease.Version)
                            {
                                var updateInfo = new ModUpdateInfo(modName, latestVersion.FriendlyName, latestRelease);
                                updateInfo.ModVersions.AddRange(groupedMods.Select(m => new ModVersionUpdateInfo(m, Modpacks)));
                                AddUpdateInfo(updateInfos, updateInfo);
                            }
                        }
                    }

                    if (updateInfos.ContainsKey(modName))
                    {
                        var latestRelease = info.GetLatestRelease();
                        if (!updateInfos[modName].ContainsKey(latestRelease.InfoFile.FactorioVersion))
                        {
                            var latestVersion = kvp.Value.MaxBy(v => v.Key, new VersionComparer()).Value.MaxBy(m => m.Version, new VersionComparer());
                            if (latestVersion.Version < latestRelease.Version)
                            {
                                var updateInfo = new ModUpdateInfo(modName, latestVersion.FriendlyName, latestRelease);
                                AddUpdateInfo(updateInfos, updateInfo);
                            }
                        }
                    }
                }
            }

            return updateInfos.Values.SelectMany(subDict => subDict.Values).ToList();
        }

        private async Task UpdateModAsyncInner(ModUpdateInfo modUpdate, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var updateFile = await ModWebsite.DownloadUpdateAsync(modUpdate.Update, GlobalCredentials.Instance.Username, token, progress, cancellationToken);
            await Mod.Add(updateFile, Mods, Modpacks, false, true);
            progress.Report(1);
        }

        private async Task UpdateModsAsyncInner(List<ModUpdateInfo> modUpdates, string token, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            try
            {
                int ModIndex = 0;
                int ModProgressIndex = 0;
                int modCount = modUpdates.Count(item => item.IsSelected);

                List<Task> downloadTasks = new List<Task>();
                List<Progress<Tuple<double, string>>> ProgressList = new List<Progress<Tuple<double, string>>>();
                //Progressing %
                List<double> ProgressCnt = new List<double>();
                //Progressing UID
                List<int> ProgressNum = new List<int>();

                void ReportProgress(double e)
                {
                    if (ProgressNum.Count > 0)
                    {
                        int minNum = 0;
                        while (minNum < modCount)
                        {
                            if (ProgressCnt[minNum] < 1 && ProgressCnt[minNum] >= 0)
                            {
                                break;
                            }
                            minNum++;
                        }
                        if (minNum >= modCount - 1)
                        {
                            minNum = modCount - 1;
                        }
                        double minPer = ProgressCnt[minNum];
                        progress.Report(new Tuple<double, string>(minPer, "(" + (minNum + 1) + " / " + modCount + ") : " + modUpdates[minNum].FriendlyName));
                    }
                }
                void SubProgress_ProgressChanged(object sender, double e)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    DownloadProgress dProgress = (DownloadProgress)sender;

                    while (ProgressCnt.Count <= modCount)
                    {
                        ProgressCnt.Add(-1);
                        ProgressNum.Add(-1);
                    }
                    ProgressCnt[dProgress.Index] = e;
                    ProgressNum[dProgress.Index] = dProgress.Index;
                }
                while (ModIndex < modCount)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    if (!modUpdates[ModIndex].IsSelected)
                    {
                        continue;
                    }
                    else
                    {
                        ModProgressIndex++;
                    }
                    while (downloadTasks.Count < Math.Min(4, modCount))
                    {
                        if (!modUpdates[ModIndex].IsSelected)
                        {
                            continue;
                        }
                        else
                        {
                            ModProgressIndex++;
                        }
                        var subProgress = new DownloadProgress(ModIndex, ReportProgress);
                        subProgress.ProgressChanged += SubProgress_ProgressChanged;
                        downloadTasks.Add(UpdateModAsyncInner(modUpdates[ModIndex], token, subProgress, cancellationToken));

                        ModIndex++;
                    }

                    Task FinishedTask = await Task.WhenAny(downloadTasks);
                    downloadTasks.Remove(FinishedTask);
                    ModIndex++;

                }
                if (cancellationToken.IsCancellationRequested) return;

                while (downloadTasks.Count > 0)
                {
                    Task FinishedTask = await Task.WhenAny(downloadTasks);
                    downloadTasks.Remove(FinishedTask);
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ModpackTemplateList.Instance.Update(Modpacks);
            ModpackTemplateList.Instance.Save();
            Mods.EvaluateDependencies();
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
                if ((modUpdates != null) && (modUpdates.Count > 0))
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
