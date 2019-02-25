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
        //private Dictionary<string, ModDependencyInfo> GetSubDict(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, Version factorioVersion)
        //{
        //    Dictionary<string, ModDependencyInfo> subDict;
        //    if (!dict.TryGetValue(factorioVersion, out subDict))
        //    {
        //        subDict = new Dictionary<string, ModDependencyInfo>();
        //        dict.Add(factorioVersion, subDict);
        //    }
        //    return subDict;
        //}

        //private void AddDependency(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, ModDependency dependency, Version factorioVersion)
        //{
        //    var subDict = GetSubDict(dict, factorioVersion);

        //    ModDependencyInfo info;
        //    if (!subDict.TryGetValue(dependency.ModName, out info))
        //    {
        //        info = new ModDependencyInfo(dependency.ModName, factorioVersion, dependency.ModVersion, dependency.IsOptional);
        //        subDict.Add(dependency.ModName, info);
        //    }
        //    else
        //    {
        //        if (dependency.HasVersionRestriction)
        //        {
        //            if ((info.Version == null) || (dependency.ModVersion > info.Version))
        //                info.Version = dependency.ModVersion;
        //        }

        //        if (!dependency.IsOptional)
        //            info.IsOptional = false;
        //    }
        //}

        //private void AddDependencies(Dictionary<Version, Dictionary<string, ModDependencyInfo>> dict, Mod mod)
        //{
        //    foreach (var dependency in mod.Dependencies)
        //    {
        //        if (!dependency.IsBase && !dependency.IsInverted && !dependency.IsPresent(Mods, mod.FactorioVersion))
        //            AddDependency(dict, dependency, mod.FactorioVersion);
        //    }
        //}

        //private List<ModDependencyInfo> GetDependencies()
        //{
        //    ICollection<Mod> selectedMods = new List<Mod>(Mods.Where(mod => mod.IsSelected));
        //    if (selectedMods.Count == 0) selectedMods = Mods;

        //    var dict = new Dictionary<Version, Dictionary<string, ModDependencyInfo>>();
        //    foreach (var mod in selectedMods)
        //        AddDependencies(dict, mod);

        //    return new List<ModDependencyInfo>(dict.SelectMany(kvp => kvp.Value.Values));
        //}

        //private async Task DownloadDependency(ModDependencyInfo dependency, IProgress<double> progress, CancellationToken cancellationToken, string token)
        //{
        //    var info = await ModWebsite.GetExtendedInfoAsync(dependency.Name);
        //    var latestRelease = info.GetLatestRelease(dependency.FactorioVersion);

        //    if ((dependency.Version != null) && (latestRelease.Version < dependency.Version))
        //    {
        //        MessageBox.Show(Window,
        //            string.Format(App.Instance.GetLocalizedMessage("DependencyUnavailable", MessageType.Information), dependency.Name, dependency.Version),
        //            App.Instance.GetLocalizedMessageTitle("DependencyUnavailable", MessageType.Information),
        //            MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        await ModWebsite.DownloadReleaseAsync(latestRelease, GlobalCredentials.Instance.Username, token, progress, cancellationToken, Mods, Modpacks);
        //    }
        //}

        //private async Task DownloadDependenciesInternal(ICollection<ModDependencyInfo> dependencies, IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken, string token)
        //{
        //    int dependencyCount = dependencies.Count;
        //    double baseProgress = 0;
        //    foreach (var dependency in dependencies)
        //    {
        //        if (cancellationToken.IsCancellationRequested) return;

        //        double dependencyProgress = 0;
        //        var subProgress = new Progress<double>(value =>
        //        {
        //            dependencyProgress = value / dependencyCount;
        //            progress.Report(new Tuple<double, string>(baseProgress + dependencyProgress, dependency.Name));
        //        });

        //        await DownloadDependency(dependency, subProgress, cancellationToken, token);

        //        baseProgress += dependencyProgress;
        //    }
        //}

        private async Task DownloadDependencies()
        {
            //var dependencies = GetDependencies();

            //var window = new DependencyDownloadWindow();
            //window.Owner = Window;
            //var viewModel = (DependencyDownloadViewModel)window.ViewModel;
            //viewModel.Dependencies = dependencies;
            //if (window.ShowDialog() == true)
            //{
            //    string token;
            //    if (GlobalCredentials.Instance.LogIn(Window, out token))
            //    {
            //        var progressWindow = new ProgressWindow() { Owner = Window };
            //        var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            //        progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("DownloadingAction");

            //        var progress = new Progress<Tuple<double, string>>(info =>
            //        {
            //            progressViewModel.Progress = info.Item1;
            //            progressViewModel.ProgressDescription = string.Format(App.Instance.GetLocalizedResourceString("DownloadingDescription"), info.Item2);
            //        });

            //        var cancellationSource = new CancellationTokenSource();
            //        progressViewModel.CanCancel = true;
            //        progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            //        try
            //        {
            //            Task closeWindowTask = null;
            //            try
            //            {
            //                var selectedDependencies = dependencies.Where(dependency => dependency.IsSelected).ToList();
            //                var downloadTask = DownloadDependenciesInternal(selectedDependencies, progress, cancellationSource.Token, token);

            //                closeWindowTask = downloadTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
            //                progressWindow.ShowDialog();

            //                await downloadTask;
            //            }
            //            finally
            //            {
            //                if (closeWindowTask != null) await closeWindowTask;
            //            }

            //            Mods.EvaluateDependencies();
            //        }
            //        catch (WebException)
            //        {
            //            MessageBox.Show(Window,
            //                App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
            //                App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
            //                MessageBoxButton.OK, MessageBoxImage.Error);
            //            return;
            //        }
            //        catch (HttpRequestException)
            //        {
            //            MessageBox.Show(Window,
            //                App.Instance.GetLocalizedMessage("InternetConnection", MessageType.Error),
            //                App.Instance.GetLocalizedMessageTitle("InternetConnection", MessageType.Error),
            //                MessageBoxButton.OK, MessageBoxImage.Error);
            //            return;
            //        }
            //    }
            //}
        }
    }
}
