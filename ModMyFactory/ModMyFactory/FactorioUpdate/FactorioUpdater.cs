using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.Web;
using ModMyFactory.Web.UpdateApi;

namespace ModMyFactory.FactorioUpdate
{
    /// <summary>
    /// Updates Factorio.
    /// </summary>
    static class FactorioUpdater
    {
        private static UpdateStep GetOptimalStep(IEnumerable<UpdateStep> updateSteps, Version from, Version maxTo)
        {
            return updateSteps.Where(step => (step.From == from) && (step.To <= maxTo)).MaxBy(step => step.To, new VersionComparer());
        }

        private static List<UpdateStep> GetStepChain(IEnumerable<UpdateStep> updateSteps, Version from, Version to)
        {
            var chain = new List<UpdateStep>();

            UpdateStep currentStep = GetOptimalStep(updateSteps, from, to);
            chain.Add(currentStep);

            while (currentStep.To < to)
            {
                UpdateStep nextStep = GetOptimalStep(updateSteps, currentStep.To, to);
                chain.Add(nextStep);

                currentStep = nextStep;
            }

            return chain;
        }

        /// <summary>
        /// Gets all valid target versions for a specified version of Factorio.
        /// </summary>
        /// <param name="versionToUpdate">The version of Factorio that is to be updated.</param>
        /// <param name="installedVersions">A collection that contains all installed versions of Factorio.</param>
        /// <param name="updateSteps">The available update steps provided by the API.</param>
        /// <returns>Returns a list of valid update targets for the specified version of Factorio.</returns>
        public static List<UpdateTarget> GetUpdateTargets(FactorioVersion versionToUpdate, ICollection<FactorioVersion> installedVersions, List<UpdateStep> updateSteps)
        {
            var targets = new List<UpdateTarget>();
            var groups = updateSteps.GroupBy(step => new Version(step.To.Major, step.To.Minor));
            foreach (var group in groups)
            {
                UpdateStep targetStep = group.MaxBy(step => step.To, new VersionComparer());
                List<UpdateStep> stepChain = GetStepChain(updateSteps, versionToUpdate.Version, targetStep.To);
                bool isValid = installedVersions.All(version => version.Version != targetStep.To);
                UpdateTarget target = new UpdateTarget(stepChain, targetStep.To, targetStep.IsStable, isValid);
                targets.Add(target);

                if (!targetStep.IsStable)
                {
                    UpdateStep stableStep = group.FirstOrDefault(step => step.IsStable);
                    if (stableStep != null)
                    {
                        stepChain = GetStepChain(updateSteps, versionToUpdate.Version, stableStep.To);
                        isValid = installedVersions.All(version => version.Version != stableStep.To);
                        target = new UpdateTarget(stepChain, stableStep.To, true, isValid);
                        targets.Add(target);
                    }
                }
            }
            return targets;
        }

        /// <summary>
        /// Downloads all required packages to update to a specified update target.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="target">The update target.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        /// <returns>Returns a list of update package files.</returns>
        private static async Task<List<FileInfo>> DownloadUpdatePackagesAsync(string username, string token, UpdateTarget target, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var packageFiles = new List<FileInfo>();

            try
            {
                int stepCount = target.Steps.Count;
                int counter = 0;
                foreach (var step in target.Steps)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var subProgress = new Progress<double>(value => progress.Report((1.0 / stepCount) * counter + (value / stepCount)));
                    var packageFile = await UpdateWebsite.DownloadUpdateStepAsync(username, token, step, subProgress, cancellationToken);
                    if (packageFile != null) packageFiles.Add(packageFile);

                    counter++;
                }
                progress.Report(1);

                if (cancellationToken.IsCancellationRequested)
                {
                    foreach (var file in packageFiles)
                    {
                        if (file.Exists)
                            file.Delete();
                    }

                    return null;
                }

                return packageFiles;
            }
            catch (Exception)
            {
                foreach (var file in packageFiles)
                {
                    if (file.Exists)
                        file.Delete();
                }

                throw;
            }
        }

        private static async Task<UpdatePackageInfo> GetUpdatePackageInfoAsync(ZipArchive archive)
        {
            return await Task.Run(() =>
            {
                UpdatePackageInfo result = null;

                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.Equals("info.json", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string json = reader.ReadToEnd();
                                result = JsonHelper.Deserialize<UpdatePackageInfo>(json);
                            }
                        }
                        int index = entry.FullName.LastIndexOf('/');
                        result.PackageDirectory = index > 0 ? entry.FullName.Substring(0, index) : string.Empty;

                        break;
                    }
                }

                if (result == null) throw new CriticalUpdaterException(UpdaterErrorType.PackageInvalid);
                return result;
            });
        }

        private static async Task AddFileAsync(FileUpdateInfo fileUpdate, FactorioVersion versionToUpdate, ZipArchive archive, string packageDirectory)
        {
            
        }

        private static async Task DeleteFileAsync(FileUpdateInfo fileUpdate, FactorioVersion versionToUpdate)
        {

        }

        private static async Task UpdateFileAsync(FileUpdateInfo fileUpdate, FactorioVersion versionToUpdate, ZipArchive archive, string packageDirectory)
        {

        }

        private static async Task ApplyUpdatePackageAsync(FactorioVersion versionToUpdate, FileInfo packageFile, IProgress<double> progress)
        {
            using (var archive = ZipFile.OpenRead(packageFile.FullName))
            {
                UpdatePackageInfo packageInfo = await GetUpdatePackageInfoAsync(archive);

                int fileCount = packageInfo.UpdatedFiles.Length;
                int counter = 0;
                foreach (var fileUpdate in packageInfo.UpdatedFiles)
                {
                    progress.Report((double)counter / fileCount);

                    switch (fileUpdate.Action)
                    {
                        case FileUpdateAction.Added:
                            await AddFileAsync(fileUpdate, versionToUpdate, archive, packageInfo.PackageDirectory);
                            break;
                        case FileUpdateAction.Removed:
                            await DeleteFileAsync(fileUpdate, versionToUpdate);
                            break;
                        case FileUpdateAction.Differs:
                            await UpdateFileAsync(fileUpdate, versionToUpdate, archive, packageInfo.PackageDirectory);
                            break;
                    }

                    counter++;
                }
                progress.Report(1);
            }
        }

        private static async Task ApplyUpdatePackagesAsync(FactorioVersion versionToUpdate, List<FileInfo> packageFiles, IProgress<double> progress)
        {
            int packageCount = packageFiles.Count;
            int counter = 0;

            var p = new Progress<double>(value => progress.Report((counter + value) / packageCount));

            foreach (var packageFile in packageFiles)
            {
                await ApplyUpdatePackageAsync(versionToUpdate, packageFile, p);

                counter++;
            }

            progress.Report(1);
        }

        /// <summary>
        /// Downloads and applies an update target to a specified version of Factorio.
        /// </summary>
        /// <param name="versionToUpdate">The version of Factorio that is going to be updated.</param>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="target">The update target.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="stageProgress">A progress object used to report the stage of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task ApplyUpdateAsync(FactorioVersion versionToUpdate, string username, string token, UpdateTarget target,
            IProgress<double> progress, IProgress<UpdaterStageInfo> stageProgress, CancellationToken cancellationToken)
        {
            stageProgress.Report(new UpdaterStageInfo(true, App.Instance.GetLocalizedResourceString("UpdatingFactorioStage1Description")));
            List<FileInfo> packageFiles = await DownloadUpdatePackagesAsync(username, token, target, progress, cancellationToken);

            try
            {
                if ((packageFiles != null) && !cancellationToken.IsCancellationRequested)
                {
                    progress.Report(0);
                    stageProgress.Report(new UpdaterStageInfo(false, App.Instance.GetLocalizedResourceString("UpdatingFactorioStage2Description")));

                    await ApplyUpdatePackagesAsync(versionToUpdate, packageFiles, progress);
                    versionToUpdate.UpdateVersion(target.TargetVersion);
                }
            }
            finally
            {
                if (packageFiles != null)
                {
                    foreach (var file in packageFiles)
                    {
                        if (file.Exists)
                            file.Delete();
                    }
                }
            }
        }
    }
}
