using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModMyFactory.Helpers;
using ModMyFactory.Models;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the mods.factorio.com website.
    /// </summary>
    static class ModWebsite
    {
        const string BaseUrl = "https://mods.factorio.com";
        const string ModsUrl = BaseUrl + "/api/mods";

        /// <summary>
        /// Gets all mods that are available online.
        /// </summary>
        /// <returns>Returns a list of online available mods.</returns>
        public static async Task<List<ModInfo>> GetModsAsync(IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var result = new List<ModInfo>();

            progress.Report(new Tuple<double, string>(0, App.Instance.GetLocalizedResourceString("ParsingFirstPageDescription")));

            string currentPageUrl = ModsUrl + "?page_size=500";
            while (!string.IsNullOrEmpty(currentPageUrl))
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                ModInfo[] pageResult = await Task.Run(() =>
                {
                    string document = WebHelper.GetDocument(currentPageUrl, null);
                    if (!string.IsNullOrEmpty(document))
                    {
                        ApiPage currentPage = JsonHelper.Deserialize<ApiPage>(document);

                        int pageNumber = currentPage.Info.PageNumber;
                        int pageCount = currentPage.Info.PageCount;
                        string progressDescription = string.Format(App.Instance.GetLocalizedResourceString("ParsingPageDescription"), pageNumber + 1, pageCount);
                        progress.Report(new Tuple<double, string>((double)pageNumber / pageCount, progressDescription));

                        currentPageUrl = currentPage.Info.Links.NextPage;
                        return currentPage.Mods;
                    }

                    currentPageUrl = null;
                    return null;
                });
                
                if (pageResult != null)
                    result.AddRange(pageResult);
            }

            return result;
        }

        private static ExtendedModInfo GetExtendedInfoInternal(string modName)
        {
            string modUrl = $"{ModsUrl}/{modName}/full";

            string document = WebHelper.GetDocument(modUrl, null);
            if (!string.IsNullOrEmpty(document))
            {
                ExtendedModInfo result = JsonHelper.Deserialize<ExtendedModInfo>(document);
                return result;
            }

            return default(ExtendedModInfo);
        }

        /// <summary>
        /// Gets extended information about a specific mod.
        /// </summary>
        /// <param name="modName">The name ot the mod to get the extended information about.</param>
        /// <returns>Returns extended information about the specified mod.</returns>
        public static async Task<ExtendedModInfo> GetExtendedInfoAsync(string modName)
        {
            ExtendedModInfo info = await Task.Run(() => GetExtendedInfoInternal(modName));
            return info;
        }

        /// <summary>
        /// Gets extended information about a specific mod.
        /// </summary>
        /// <param name="mod">The mod to get the extended information about.</param>
        /// <returns>Returns extended information about the specified mod.</returns>
        public static async Task<ExtendedModInfo> GetExtendedInfoAsync(ModInfo mod)
        {
            return await GetExtendedInfoAsync(mod.Name);
        }

        /// <summary>
        /// Gets extended information about a specific mod.
        /// </summary>
        /// <param name="mod">The mod to get the extended information about.</param>
        /// <returns>Returns extended information about the specified mod.</returns>
        public static async Task<ExtendedModInfo> GetExtendedInfoAsync(Mod mod)
        {
            return await GetExtendedInfoAsync(mod.Name);
        }

        //private static bool IsEmail(string username)
        //{
        //    string[] parts = username.Split('@');
        //    if (parts.Length != 2) return false;

        //    string domain = parts[1];
        //    int dotIndex = domain.LastIndexOf('.');
        //    return (dotIndex < (domain.Length - 1));
        //}

        private static Uri BuildUrl(ModRelease release, string username, string token)
        {
            return new Uri($"{BaseUrl}{release.DownloadUrl}?username={username}&token={token}");
        }

        /// <summary>
        /// Downloads a mod.
        /// </summary>
        /// <param name="release">The mods release to be downloaded.</param>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        /// <param name="parentCollection">The collection to contain the mods.</param>
        /// <param name="modpackCollection">The collection containing all modpacks.</param>
        public static async Task<Mod> DownloadReleaseAsync(ModRelease release, string username, string token, IProgress<double> progress, CancellationToken cancellationToken,
            ModCollection parentCollection, ModpackCollection modpackCollection)
        {
            DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory(release.InfoFile.FactorioVersion);
            if (!modDirectory.Exists) modDirectory.Create();

            var downloadUrl = BuildUrl(release, username, token);
            Debug.Print(token);
            var file = new FileInfo(Path.Combine(modDirectory.FullName, release.FileName));

            try
            {
                await WebHelper.DownloadFileAsync(downloadUrl, null, file, progress, cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (ModFile.TryLoadFromFile(file, out ModFile modFile))
                    {
                        if (modFile.InfoFile.FactorioVersion == release.InfoFile.FactorioVersion)
                        {
                            return await Mod.Add(modFile, parentCollection, modpackCollection, false);
                        }
                    }

                    throw new InvalidOperationException("The server sent an invalid mod file.");
                }
            }
            catch (Exception)
            {
                if (file.Exists) file.Delete();

                throw;
            }

            return null;
        }

        /// <summary>
        /// Downloads a mod.
        /// </summary>
        /// <param name="release">The mods release to be downloaded.</param>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="fileName">The destination file name.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task<Mod> DownloadReleaseToFileAsync(ModRelease release, string username, string token, string fileName, IProgress<double> progress, CancellationToken cancellationToken)
        {
            DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory(release.InfoFile.FactorioVersion);
            if (!modDirectory.Exists) modDirectory.Create();

            var downloadUrl = BuildUrl(release, username, token);
            Debug.Print(token);
            var modFile = new FileInfo(fileName);

            try
            {
                await WebHelper.DownloadFileAsync(downloadUrl, null, modFile, progress, cancellationToken);
            }
            catch (Exception)
            {
                if (modFile.Exists) modFile.Delete();

                throw;
            }

            return null;
        }

        /// <summary>
        /// Downloads a mod for updating.
        /// </summary>
        /// <param name="release">The mods release to be downloaded.</param>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        public static async Task<FileInfo> UpdateReleaseAsync(ModRelease release, string username, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory(release.InfoFile.FactorioVersion);
            if (!modDirectory.Exists) modDirectory.Create();

            var downloadUrl = BuildUrl(release, username, token);
            var modFile = new FileInfo(Path.Combine(modDirectory.FullName, release.FileName));

            await WebHelper.DownloadFileAsync(downloadUrl, null, modFile, progress, cancellationToken);
            return cancellationToken.IsCancellationRequested ? null : modFile;
        }
    }
}
