using ModMyFactory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
        /// <summary>
        /// Logs in at the website.
        /// </summary>
        /// <param name="container">The cookie container to store the session cookie in.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The users password.</param>
        /// <returns>Returns false if the login failed, otherwise true.</returns>
        public static bool LogIn(CookieContainer container, string username, string password)
        {
            const string loginPage = "https://www.factorio.com/login";
            const string pattern = "[0-9]{10}##[0-9a-f]{40}";

            string document;

            // Get a csrf token.
            if (!WebHelper.TryGetDocument(loginPage, container, out document)) return false;
            MatchCollection matches = Regex.Matches(document, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if (matches.Count != 1) return false;
            string csrfToken = matches[0].Value;

            // Log in using the token and credentials.
            string content = $"csrf_token={csrfToken}&username_or_email={username}&password={password}&action=Login";
            if (!WebHelper.TryGetDocument(loginPage, container, content, out document)) return false;
            if (!document.Contains("logout")) return false;

            return true;
        }

        /// <summary>
        /// Ensures a session is logged in at the website.
        /// </summary>
        /// <param name="container">The cookie container the session cookie is stored in.</param>
        /// <returns>Returns true if the session is logged in, otherwise false.</returns>
        public static bool EnsureLoggedIn(CookieContainer container)
        {
            const string mainPage = "https://www.factorio.com";

            string document;
            if (!WebHelper.TryGetDocument(mainPage, container, out document)) return false;
            return document.Contains("logout");
        }

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
        /// <param name="container">The cookie container the session cookie is stored in.</param>
        /// <param name="versions">Out. The list of available Factorio versions.</param>
        /// <returns>Returns false if the version list could not be retrieved, otherwise true.</returns>
        public static bool GetVersions(CookieContainer container, out List<FactorioOnlineVersion> versions)
        {
            const string downloadPage = "https://www.factorio.com/download";
            const string experimentalDownloadPage = "https://www.factorio.com/download/experimental";
            const string pattern = @"<h3> *(?<version>[0-9]+\.[0-9]+\.[0-9]+) *(?<modifier>\([a-z]+\)) *</h3>";
            string[] allowedModifiers = { "(alpha)" };

            string document;
            versions = new List<FactorioOnlineVersion>();

            // Get stable versions.
            if (!WebHelper.TryGetDocument(downloadPage, container, out document)) return false;
            MatchCollection matches = Regex.Matches(document, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                string versionString = match.Groups["version"].Value;
                string modifierString = match.Groups["modifier"].Value;
                Version version = Version.Parse(versionString);

                if (allowedModifiers.Contains(modifierString) && VersionCompatibleWithPlatform(version))
                {
                    var factorioVersion = new FactorioOnlineVersion(version, modifierString);
                    versions.Add(factorioVersion);
                }
            }

            // Get experimental versions.
            if (!WebHelper.TryGetDocument(experimentalDownloadPage, container, out document)) return false;
            matches = Regex.Matches(document, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                string versionString = match.Groups["version"].Value;
                string modifierString = match.Groups["modifier"].Value;

                if (allowedModifiers.Contains(modifierString))
                {
                    var version = new FactorioOnlineVersion(Version.Parse(versionString), modifierString + " (experimental)");
                    versions.Add(version);
                }
            }

            return true;
        }

        /// <summary>
        /// Downloads a Factorio package.
        /// </summary>
        /// <param name="version">The version of Factorio to be downloaded.</param>
        /// <param name="downloadDirectory">The directory the package is downloaded to.</param>
        /// <param name="container">The cookie container the session cookie is stored in.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task<FactorioVersion> DownloadFactorioPackageAsync(FactorioOnlineVersion version, DirectoryInfo downloadDirectory, CookieContainer container, IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (!downloadDirectory.Exists) downloadDirectory.Create();

            string filePath = Path.Combine(downloadDirectory.FullName, "package.zip");
            var file = new FileInfo(filePath);

            await WebHelper.DownloadFileAsync(version.DownloadUrl, container, file, progress, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
            {
                progress.Report(2);
                FactorioVersion factorioVersion = await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(file.FullName, downloadDirectory.FullName);

                    string versionString = version.Version.ToString(3);
                    var versionDirectory = downloadDirectory.EnumerateDirectories($"Factorio_{versionString}*").First();
                    versionDirectory.MoveTo(Path.Combine(downloadDirectory.FullName, versionString));
                    file.Delete();

                    return new FactorioVersion(versionDirectory, version.Version);
                });

                return factorioVersion;
            }

            return null;
        }
    }
}
