using ModMyFactory.Models;
using ModMyFactory.Views;
using ModMyFactory.Web;
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
        private Dictionary<string, ModDependencyInfo> GetSubDict(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, Version factorioVersion)
        {
            Dictionary<string, ModDependencyInfo> subDict;
            if (!dict.TryGetValue(factorioVersion, out subDict))
            {
                subDict = new Dictionary<string, ModDependencyInfo>();
                dict.Add(factorioVersion, subDict);
            }
            return subDict;
        }

        private void AddDependency(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, ModDependency dependency, Version factorioVersion)
        {
            var subDict = GetSubDict(dict, factorioVersion);

            ModDependencyInfo info;
            if (!subDict.TryGetValue(dependency.ModName, out info))
            {
                info = new ModDependencyInfo(dependency.ModName, factorioVersion, dependency.ModVersion, dependency.IsOptional);
                subDict.Add(dependency.ModName, info);
            }
            else
            {
                if (dependency.HasVersionRestriction)
                {
                    if ((info.Version == null) || (dependency.ModVersion > info.Version))
                        info.Version = dependency.ModVersion;
                }

                if (!dependency.IsOptional)
                    info.IsOptional = false;
            }
        }

        private void AddDependencies(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, Mod mod)
        {
            foreach (var dependency in mod.Dependencies)
            {
                if (!dependency.IsBase && !dependency.IsInverted && !dependency.IsMet(Mods, mod.FactorioVersion))
                    AddDependency(dict, dependency, mod.FactorioVersion);
            }
        }

        private List<ModDependencyInfo> GetDependencies()
        {
            ICollection<Mod> selectedMods = new List<Mod>(Mods.Where(mod => mod.IsSelected));
            if (selectedMods.Count == 0) selectedMods = Mods;

            var dict = new Dictionary<Version, Dictionary<string, ModDependencyInfo>>();
            foreach (var mod in selectedMods)
                AddDependencies(dict, mod);

            return new List<ModDependencyInfo>(dict.SelectMany(kvp => kvp.Value.Values));
        }

        private async Task DownloadDependency(ModDependencyInfo dependency, IProgress<double> progress, CancellationToken cancellationToken, string token)
        {
            var info = await ModWebsite.GetExtendedInfoAsync(dependency.Name);
            var latestRelease = info.GetLatestRelease(dependency.FactorioVersion);

            if ((dependency.Version != null) && (latestRelease.Version < dependency.Version))
            {
                MessageBox.Show(Window,
                    string.Format(App.Instance.GetLocalizedMessage("DependencyUnavailable", MessageType.Information), dependency.Name, dependency.Version),
                    App.Instance.GetLocalizedMessageTitle("DependencyUnavailable", MessageType.Information),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await ModWebsite.DownloadReleaseAsync(latestRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken, Mods, Modpacks);
            }
            progress.Report(1);
        }

        private async Task DownloadDependenciesInternal(IList<ModDependencyInfo> dependencies, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken, string token)
        {
            int dependencyIndex = 0;
            int dependencyCount = dependencies.Count;

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
                    while (minNum < dependencyCount)
                    {
                        if (ProgressCnt[minNum] < 1 && ProgressCnt[minNum] >= 0)
                        {
                            break;
                        }
                        minNum++;
                    }
                    if (minNum >= dependencyCount - 1)
                    {
                        minNum = dependencyCount - 1;
                    }
                    double minPer = ProgressCnt[minNum];
                    progress.Report(new Tuple<double, string>(minPer, "("+ (minNum + 1) + " / "+ dependencyCount + ") : "+dependencies[minNum].Name));
                }
            }
            void SubProgress_ProgressChanged(object sender, double e)
            {
                if (cancellationToken.IsCancellationRequested) return;

                DownloadProgress dProgress = (DownloadProgress)sender;

                while (ProgressCnt.Count <= dependencyCount)
                {
                    ProgressCnt.Add(-1);
                    ProgressNum.Add(-1);
                }
                ProgressCnt[dProgress.Index] = e;
                ProgressNum[dProgress.Index] = dProgress.Index;
            }
            while (dependencyIndex < dependencyCount)
            {
                if (cancellationToken.IsCancellationRequested) break;


                while (downloadTasks.Count < Math.Min(4, dependencyCount))
                {
                    var subProgress = new DownloadProgress(dependencyIndex, ReportProgress);
                    subProgress.ProgressChanged += SubProgress_ProgressChanged;
                    downloadTasks.Add(DownloadDependency(dependencies[dependencyIndex], subProgress, cancellationToken, token));

                    dependencyIndex++;
                }

                Task FinishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(FinishedTask);
                dependencyIndex++;

            }
            if (cancellationToken.IsCancellationRequested) return;

            while (downloadTasks.Count > 0)
            {
                Task FinishedTask = await Task.WhenAny(downloadTasks);
                downloadTasks.Remove(FinishedTask);
            }
           
        }

        private async Task DownloadDependencies()
        {
            var dependencies = GetDependencies();

            var window = new DependencyDownloadWindow();
            window.Owner = Window;
            var viewModel = (DependencyDownloadViewModel)window.ViewModel;
            viewModel.Dependencies = dependencies;
            if (window.ShowDialog() == true)
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

                        Mods.EvaluateDependencies();
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
                }
            }
        }
    }
}
