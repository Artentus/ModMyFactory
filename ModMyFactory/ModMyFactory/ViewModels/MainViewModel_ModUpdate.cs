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
        private void SetLatestInstalledVersion(Dictionary<string, Mod> latestInstalledVersions, Mod mod)
        {
            Mod latestInstalledVersion;
            if (latestInstalledVersions.TryGetValue(mod.Name, out latestInstalledVersion))
            {
                if (mod.Version > latestInstalledVersion.Version)
                    latestInstalledVersions[mod.Name] = mod;
            }
            else
            {
                latestInstalledVersions.Add(mod.Name, mod);
            }
        }

        private async Task<ExtendedModInfo> GetInfoAsync(Dictionary<string, ExtendedModInfo> infos, string modName)
        {
            ExtendedModInfo info;
            if (!infos.TryGetValue(modName, out info))
            {
                info = await ModWebsite.GetExtendedInfoAsync(modName);
                infos.Add(modName, info);
            }

            return info;
        }

        private async Task<List<ModUpdateInfo>> GetModUpdatesAsync(IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var result = new List<ModUpdateInfo>();

            if (App.Instance.Settings.ManagerMode == ManagerMode.PerFactorioVersion)
            {
                var latestInstalledVersions = new Dictionary<string, Mod>();
                var infos = new Dictionary<string, ExtendedModInfo>();
                
                int modCount = Mods.Count;
                int index = 0;
                foreach (var mod in Mods)
                {
                    progress.Report(new Tuple<double, string>((double)index / modCount, mod.FriendlyName));

                    SetLatestInstalledVersion(latestInstalledVersions, mod);
                    ExtendedModInfo info = await GetInfoAsync(infos, mod.Name);

                    var release = info.LatestRelease(mod.FactorioVersion);
                    if ((release != null) && (release.Version > mod.Version))
                            result.Add(new ModUpdateInfo(mod, release, false));

                    index++;
                }

                foreach (var kvp in latestInstalledVersions)
                {
                    var mod = kvp.Value;
                    var info = infos[kvp.Key];

                    var latestRelease = info.LatestRelease();
                    if (latestRelease.InfoFile.FactorioVersion > mod.FactorioVersion)
                        result.Add(new ModUpdateInfo(mod, latestRelease, true));
                }
            }
            else
            {
                int modCount = Mods.Count;
                int index = 0;
                foreach (var mod in Mods)
                {
                    progress.Report(new Tuple<double, string>((double)index / modCount, mod.FriendlyName));

                    var info = await ModWebsite.GetExtendedInfoAsync(mod.Name);
                    var latestRelease = info.LatestRelease();
                    if ((latestRelease != null) && (latestRelease.Version > mod.Version))
                        result.Add(new ModUpdateInfo(mod, latestRelease, false));

                    index++;
                }
            }

            return result;
        }

        private async Task UpdateModAsyncInner(ModUpdateInfo modUpdate, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var updateFile = await ModWebsite.DownloadUpdateAsync(modUpdate.Update, GlobalCredentials.Instance.Username, token, progress, cancellationToken);
            if (modUpdate.CreateNewMod)
                await Mod.Add(updateFile, Mods, Modpacks, false, true);
            else
                await modUpdate.Mod.UpdateAsync(updateFile);
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
                        progress.Report(new Tuple<double, string>(baseProgressValue + modProgressValue, modUpdate.FriendlyName));
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
