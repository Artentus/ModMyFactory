using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the Factorio.com website.
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

                if (allowedModifiers.Contains(modifierString))
                {
                    var version = new FactorioOnlineVersion(Version.Parse(versionString), modifierString);
                    versions.Add(version);
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

        public static async Task DownloadFactorioPackage(FactorioOnlineVersion version, CookieContainer container, IProgress<double> progress, CancellationToken cancellationToken)
        {
            string directoryPath = Path.Combine(Environment.CurrentDirectory, "Factorio");
            var directory = new DirectoryInfo(directoryPath);
            if (!directory.Exists) directory.Create();

            string filePath = Path.Combine(directory.FullName, "package.zip");
            var file = new FileInfo(filePath);

            await WebHelper.DownloadFile(version.DownloadUrl, container, file, progress, cancellationToken);
            progress.Report(2);
            await Task.Run(() => ZipFile.ExtractToDirectory(file.FullName, directory.FullName));
            file.Delete();
        }
    }
}
