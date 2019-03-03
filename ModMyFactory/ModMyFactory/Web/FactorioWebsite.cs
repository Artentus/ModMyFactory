using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the factorio.com website.
    /// </summary>
    static class FactorioWebsite
    {
        private static bool VersionListContains(List<FactorioOnlineVersion> versionList, Version version)
        {
            return versionList.Any(item => item.Version == version);
        }
        
        private static void GetVersionsFromUrl(string url, bool isExperimental, List<FactorioOnlineVersion> versionList)
        {
            const string pattern = @"<h3> *(?<version>\d+\.\d+\.\d+) +\(.+\) *<\/h3>";

            string document = WebHelper.GetDocument(url);
            var matches = Regex.Matches(document, pattern);
            foreach (Match match in matches)
            {
                string versionString = match.Groups["version"].Value;
                var version = Version.Parse(versionString);

                if (!VersionListContains(versionList, version))
                {
                    var onlineVersion = new FactorioOnlineVersion(version, isExperimental);
                    versionList.Add(onlineVersion);
                }
            }
        }

        /// <summary>
        /// Reads the Factorio version list.
        /// </summary>
        /// <returns>Returns the list of available Factorio versions or null if the operation was unsucessful.</returns>
        public static async Task<List<FactorioOnlineVersion>> GetVersionsAsync()
        {
            return await Task.Run(() =>
            {
                var result = new List<FactorioOnlineVersion>();
                GetVersionsFromUrl("https://factorio.com/download-headless", false, result);
                GetVersionsFromUrl("https://factorio.com/download-headless/experimental", true, result);
                return result;
            });
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
            await WebHelper.DownloadFileAsync(new Uri(url), file, progress, cancellationToken);

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
