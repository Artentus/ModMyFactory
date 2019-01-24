using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        private static ApiPage DownloadPage(int pageSize, int page)
        {
            string pageUrl = $"{ModsUrl}?page_size={pageSize}";
            if (page > 1) pageUrl += $"&page={page}";

            string document = WebHelper.GetDocument(pageUrl, null);
            return JsonHelper.Deserialize<ApiPage>(document);
        }

        private static ApiPage DownloadFirstPage(int pageSize)
        {
            return DownloadPage(pageSize, 1);
        }

        private static IEnumerable<ApiPage> DownloadAllPages(int pageSize)
        {
            var firstPage = DownloadFirstPage(pageSize);
            int pageCount = firstPage.Info.PageCount;

            var pages = new ConcurrentBag<ApiPage>();
            pages.Add(firstPage);
            if (pageCount == 1) return pages;
            
            Parallel.For(2, pageCount + 1, pageIndex =>
            {
                var page = DownloadPage(pageSize, pageIndex);
                pages.Add(page);
            });

            return pages;
        }

        /// <summary>
        /// Gets all mods that are available online.
        /// </summary>
        /// <returns>Returns a list of online available mods.</returns>
        public static async Task<List<ModInfo>> GetModsAsync(ModCollection installedMods, int pageSize = 500)
        {
            List<ModInfo> mods = null;

            var pages = await Task.Run(() => DownloadAllPages(pageSize));
            foreach (var page in pages)
            {
                if (mods == null)
                    mods = new List<ModInfo>(page.Info.ModCount + 50);

                mods.AddRange(page.Mods);
            }

            foreach (var installedMod in installedMods)
            {
                if (!mods.Any(mod => mod.Name == installedMod.Name))
                {
                    try
                    {
                        var info = await GetExtendedInfoAsync(installedMod);
                        mods.Add(info);
                    }
                    catch (WebException ex)
                    {
                        if (ex.Status != WebExceptionStatus.ProtocolError) throw;
                    }
                }
            }

            if (mods == null) mods = new List<ModInfo>();
            return mods;
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
            return await Task.Run(() => GetExtendedInfoInternal(modName));
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
            if(username == null || token == null)
            {
                return null;
            }
            DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory(release.InfoFile.FactorioVersion);
            if (!modDirectory.Exists) modDirectory.Create();

            var downloadUrl = BuildUrl(release, username, token);
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
        public static async Task<ModFile> DownloadUpdateAsync(ModRelease release, string username, string token, IProgress<double> progress, CancellationToken cancellationToken)
        {
            DirectoryInfo modDirectory = App.Instance.Settings.GetModDirectory(release.InfoFile.FactorioVersion);
            if (!modDirectory.Exists) modDirectory.Create();

            var downloadUrl = BuildUrl(release, username, token);
            var file = new FileInfo(Path.Combine(modDirectory.FullName, release.FileName));
            await WebHelper.DownloadFileAsync(downloadUrl, null, file, progress, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return null;

            ModFile modFile;
            if (!ModFile.TryLoadFromFile(file, out modFile))
                throw new InvalidOperationException("The server sent an invalid mod file.");
            return modFile;
        }

        /// <summary>
        /// Opens a mods page in the default web browser.
        /// </summary>
        /// <param name="mod">The mod to navigate to.</param>
        public static void OpenModInBrowser(Mod mod)
        {
            if (mod == null) return;

            string url = $"{BaseUrl}/mod/{mod.Name}";
            Process.Start(url);
        }
    }
}
