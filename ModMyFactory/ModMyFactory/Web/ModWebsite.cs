using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModMyFactory.Helpers;
using ModMyFactory.Web.ModApi;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the mods.factorio.com website.
    /// </summary>
    static class ModWebsite
    {
        const string BaseUrl = "https://mods.factorio.com";
        const string ModsUrl =  BaseUrl + "/api/mods";

        /// <summary>
        /// Gets all mods that are available online.
        /// </summary>
        /// <returns>Returns a list of online available mods.</returns>
        public static async Task<List<ModInfo>> GetModsAsync(IProgress<Tuple<double, string>> progress, CancellationToken cancellationToken)
        {
            var result = new List<ModInfo>();

            string currentPageUrl = ModsUrl;
            while (!string.IsNullOrEmpty(currentPageUrl))
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                ModInfo[] pageResult = await Task.Run<ModInfo[]>(() =>
                {
                    string document;
                    if (WebHelper.TryGetDocument(currentPageUrl, null, out document))
                    {
                        ApiPage currentPage = JsonHelper.Deserialize<ApiPage>(document);
                        int pageNumber = currentPage.Info.PageNumber;
                        int pageCount = currentPage.Info.PageCount;
                        progress.Report(new Tuple<double, string>((double)pageNumber / pageCount, $"Parsing page {pageNumber + 1} of {pageCount}."));

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

        /// <summary>
        /// Gets extended information about a specific mod.
        /// </summary>
        /// <param name="mod">The mod to get the extended information about.</param>
        /// <returns>Returns extended information about the specified mod.</returns>
        public static async Task<ExtendedModInfo> GetExtendedInfo(ModInfo mod)
        {
            string modUrl = $"{ModsUrl}/{mod.Name}";

            ExtendedModInfo info = await Task.Run<ExtendedModInfo>(() =>
            {
                string document;
                if (WebHelper.TryGetDocument(modUrl, null, out document))
                {
                    ExtendedModInfo result = JsonHelper.Deserialize<ExtendedModInfo>(document);
                    return result;
                }

                return default(ExtendedModInfo);
            });

            return info;
        }
    }
}
