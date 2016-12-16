using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ModMyFactory.ViewModels;
using ModMyFactory.Views;
using ModMyFactory.Web;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.Helpers
{
    static class ModHelper
    {
        public static async Task<List<ModInfo>> FetchMods(Window progressOwner)
        {
            var progressWindow = new ProgressWindow() { Owner = progressOwner };
            var progressViewModel = (ProgressViewModel)progressWindow.ViewModel;
            progressViewModel.ActionName = App.Instance.GetLocalizedResourceString("FetchingModsAction");
            progressViewModel.CanCancel = true;

            var progress = new Progress<Tuple<double, string>>(value =>
            {
                progressViewModel.Progress = value.Item1;
                progressViewModel.ProgressDescription = value.Item2;
            });
            var cancellationSource = new CancellationTokenSource();
            progressViewModel.CancelRequested += (sender, e) => cancellationSource.Cancel();

            Task<List<ModInfo>> fetchModsTask = ModWebsite.GetModsAsync(progress, cancellationSource.Token);

            Task closeWindowTask = fetchModsTask.ContinueWith(t => progressWindow.Dispatcher.Invoke(progressWindow.Close));
            progressWindow.ShowDialog();

            List<ModInfo> modInfos = await fetchModsTask;
            await closeWindowTask;

            return cancellationSource.IsCancellationRequested ? null : modInfos;
        }
    }
}
