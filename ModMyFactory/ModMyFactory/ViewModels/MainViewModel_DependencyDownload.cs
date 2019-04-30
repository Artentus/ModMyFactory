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
        private async Task<ExtendedModInfo> GetModInfoAsync(Dictionary<string, ExtendedModInfo> infos, string modName)
        {
            ExtendedModInfo info;
            if (!infos.TryGetValue(modName, out info))
            {
                try
                {
                    info = await ModWebsite.GetExtendedInfoAsync(modName);
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    info = null;
                }

                infos.Add(modName, info);
            }

            return info;
        }

        private Dictionary<Version, ModDependencyInfo> GetSubDict(Dictionary<string, Dictionary<Version, ModDependencyInfo>> dict, string modName)
        {
            Dictionary<Version, ModDependencyInfo> subDict;
            if (!dict.TryGetValue(modName, out subDict))
            {
                subDict = new Dictionary<Version, ModDependencyInfo>();
                dict.Add(modName, subDict);
            }
            return subDict;
        }

        private async Task AddDependency(Dictionary<string, Dictionary<Version, ModDependencyInfo>> dict, Dictionary<string, ExtendedModInfo> infos, ModDependency dependency, Version factorioVersion)
        {
            var modInfo = await GetModInfoAsync(infos, dependency.ModName);
            if ((modInfo != null) && dependency.IsPresent(modInfo, factorioVersion, out var release))
            {
                var subDict = GetSubDict(dict, dependency.ModName);

                ModDependencyInfo info;
                if (!subDict.TryGetValue(release.Version, out info))
                {
                    info = new ModDependencyInfo(release, dependency.ModName, factorioVersion, dependency.IsOptional);
                    subDict.Add(release.Version, info);
                }
                else
                {
                    if (!dependency.IsOptional) info.IsOptional = false;
                }
            }
        }

        private async Task AddDependencies(Dictionary<string, Dictionary<Version, ModDependencyInfo>> dict, Dictionary<string, ExtendedModInfo> infos, Mod mod, CancellationToken cancellationToken)
        {
            foreach (var dependency in mod.Dependencies)
            {
                if (cancellationToken.IsCancellationRequested) return;

                if (!(dependency.IsBase || dependency.IsInverted || dependency.IsHidden || dependency.IsPresent(Mods, mod.FactorioVersion)))
                    await AddDependency(dict, infos, dependency, mod.FactorioVersion);
            }
        }

        private async Task<List<ModDependencyInfo>> GetDependencies(CancellationToken cancellationToken)
        {
            ICollection<Mod> selectedMods = new List<Mod>(Mods.Where(mod => mod.IsSelected));
            if (selectedMods.Count == 0) selectedMods = Mods;

            var dict = new Dictionary<string, Dictionary<Version, ModDependencyInfo>>();
            var infos = new Dictionary<string, ExtendedModInfo>();
            foreach (var mod in selectedMods)
            {
                if (cancellationToken.IsCancellationRequested) break;

                await AddDependencies(dict, infos, mod, cancellationToken);
            }

            return new List<ModDependencyInfo>(dict.SelectMany(kvp => kvp.Value.Values));
        }

        private async Task DownloadDependency(ModDependencyInfo dependency, IProgress<double> progress, CancellationToken cancellationToken, string token)
        {
            var release = dependency.Release;
            await ModWebsite.DownloadReleaseAsync(release, GlobalCredentials.Instance.Username, token, progress, cancellationToken, Mods, Modpacks);
        }

        private async Task DownloadDependenciesInternal(ICollection<ModDependencyInfo> dependencies, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken, string token)
        {
            Mods.BeginUpdate();

            int dependencyCount = dependencies.Count;
            double baseProgress = 0;
            foreach (var dependency in dependencies)
            {
                if (cancellationToken.IsCancellationRequested) return;

                double dependencyProgress = 0;
                var subProgress = new Progress<double>(value =>
                {
                    dependencyProgress = value / dependencyCount;
                    progress.Report(new Tuple<double, string>(baseProgress + dependencyProgress, dependency.Name));
                });

                await DownloadDependency(dependency, subProgress, cancellationToken, token);

                baseProgress += dependencyProgress;
            }

            Mods.EndUpdate();
        }

        private async Task DownloadDependencies()
        {
            var progressWindow = new ProgressWindow() { Owner = Window };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("FetchingDependenciesAction");
            progressViewModel.IsIndeterminate = true;

            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CanCancel = true;
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            List<ModDependencyInfo> dependencies = null;

            try
            {
                Task closeWindowTask = null;
                try
                {
                    var getDependenciesTask = GetDependencies(cancellationSource.Token);

                    closeWindowTask = getDependenciesTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                    progressWindow.ShowDialog();

                    dependencies = await getDependenciesTask;
                }
                finally
                {
                    if (closeWindowTask != null) await closeWindowTask;
                }
            }
            catch (Exception ex) when ((ex is WebException) || (ex is HttpRequestException))
            {
                MessageBox.Show(Window,
                    App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                    App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!cancellationSource.IsCancellationRequested)
            {
                var window = new DependencyDownloadWindow() { Owner = Window };
                var viewModel = (DependencyDownloadViewModel)window.ViewModel;
                viewModel.Dependencies = dependencies;

                if (window.ShowDialog() == true)
                {
                    string token;
                    if (GlobalCredentials.Instance.LogIn(Window, out token))
                    {
                        progressWindow = new ProgressWindow() { Owner = Window };
                        progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
                        progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");
                        progressViewModel.CanCancel = false;

                        var progress = new Progress<Tuple<double, string>>(info =>
                        {
                            progressViewModel.Progress = info.Item1;
                            progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), info.Item2);
                        });

                        cancellationSource = new CancellationTokenSource();
                        progressViewModel.CanCancel = true;
                        progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

                        try
                        {
                            Task closeWindowTask = null;
                            try
                            {
                                var selectedDependencies = dependencies.Where(dependency => dependency.IsSelected).ToList();
                                var downloadTask = DownloadDependenciesInternal(selectedDependencies, progress, cancellationSource.Token, token);

                                closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
                                progressWindow.ShowDialog();

                                await downloadTask;
                            }
                            finally
                            {
                                if (closeWindowTask != null) await closeWindowTask;
                            }
                        }
                        catch (Exception ex) when ((ex is WebException) || (ex is HttpRequestException))
                        {
                            MessageBox.Show(Window,
                                App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
                                App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }
        }
    }
}
