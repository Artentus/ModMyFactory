using ModMyFactory.Helpers;
using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the factorio.com website.
    /// </summary>
    static class FactorioWebsite
    {
        /// <summary>
        /// Reads the Factorio version list.
        /// </summary>
        /// <returns>Returns the list of available Factorio versions or null if the operation was unsucessful.</returns>
        public static async Task<List<FactorioOnlineVersion>> GetVersionsAsync(string username, string token)
        {
            var updateInfo = await UpdateWebsite.GetUpdateInfoAsync(username, token);
            if (updateInfo == null) return null;

            var versions = new List<FactorioOnlineVersion>();

            var groups = updateInfo.Package.GroupBy(item => new Version(item.To.Major, item.To.Minor));
            var latestMainVersion = groups.Select(group => group.Key).Max();
            foreach (var group in groups)
            {
                if (group.Key == latestMainVersion)
                {
                    var latestStable = group.Where(item => item.IsStable).MaxBy(item => item.To);
                    if (latestStable != null) versions.Add(new FactorioOnlineVersion(latestStable.To, false));

                    var latestExperimental = group.MaxBy(item => item.To);
                    if ((latestExperimental != null) && (latestExperimental != latestStable)) versions.Add(new FactorioOnlineVersion(latestExperimental.To, true));
                }
                else
                {
                    var latestStable = group.MaxBy(item => item.To);
                    if (latestStable != null) versions.Add(new FactorioOnlineVersion(latestStable.To, false));
                }
            }

            return versions;
        }

        /// <summary>
        /// Downloads Factorio.
        /// </summary>
        /// <param name="version">The version of Factorio to be downloaded.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task<FactorioVersion> DownloadFactorioAsync(FactorioOnlineVersion version, string username, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var factorioDirectory = App.Instance.Settings.GetFactorioDirectory();
            if (!factorioDirectory.Exists) factorioDirectory.Create();
            
            var file = new FileInfo(Path.Combine(factorioDirectory.FullName, "package.zip"));
            string url = version.DownloadUrl + $"?username={username}&token={token}";
            await WebHelper.DownloadFileAsync(new Uri(url), null, file, progress, cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested) return null;
                progress.Report(2);

                if (!FactorioFile.TryLoad(file, out var factorioFile)) return null;
                var factorioFolder = await FactorioFolder.FromFileAsync(factorioFile, factorioDirectory);
                return new FactorioVersion(factorioFolder);
            }
            finally
            {
                if (file.Exists)
                    file.Delete();
            }
        }
    }
}
