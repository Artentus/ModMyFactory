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
        private static bool VersionCompatibleWithPlatform(Version version)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return true;
            }
            else
            {
                // 32 bit no longer supported as of version 0.15.
                return version < new Version(0, 15);
            }
        }

        /// <summary>
        /// Reads the Factorio version list.
        /// </summary>
        /// <returns>Returns the list of available Factorio versions or null if the operation was unsucessful.</returns>
        public static async Task<List<FactorioOnlineVersion>> GetVersionsAsync()
        {
            const string downloadPage = "https://www.factorio.com/download-demo";
            const string experimentalDownloadPage = "https://www.factorio.com/download-demo/experimental";
            const string pattern = @"<h3> *(?<version>[0-9]+\.[0-9]+\.[0-9]+) *\(.+\) *</h3>";

            var versions = new List<FactorioOnlineVersion>();

            try
            {
                // Get stable versions.
                string document = await Task.Run(() => WebHelper.GetDocument(downloadPage, null));
                MatchCollection matches = Regex.Matches(document, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string versionString = match.Groups["version"].Value;
                    Version version = Version.Parse(versionString);

                    if (VersionCompatibleWithPlatform(version))
                    {
                        var factorioVersion = new FactorioOnlineVersion(version, false);
                        versions.Add(factorioVersion);
                    }
                }

                // Get experimental versions.
                document = await Task.Run(() => WebHelper.GetDocument(experimentalDownloadPage, null));
                matches = Regex.Matches(document, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    string versionString = match.Groups["version"].Value;
                    Version version = Version.Parse(versionString);

                    if (VersionCompatibleWithPlatform(version))
                    {
                        if (versions.All(item => item.Version != version))
                        {
                            var factorioVersion = new FactorioOnlineVersion(version, true);
                            versions.Add(factorioVersion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return null;
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
