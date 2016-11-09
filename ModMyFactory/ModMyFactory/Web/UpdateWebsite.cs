using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModMyFactory.Helpers;
using ModMyFactory.Web.UpdateApi;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the updater.factorio.com website.
    /// </summary>
    static class UpdateWebsite
    {
        const string BaseUrl = "https://updater.factorio.com";
        const int ApiVersion = 2;

        /// <summary>
        /// Get all available Factorio updates.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <returns>Returns information about all available updates for Factorio.</returns>
        public static UpdateInfo GetUpdateInfo(string username, string token)
        {
            string url = $"{BaseUrl}/get-available-versions?username={username}&token={token}&apiVersion={ApiVersion}";
            string document = WebHelper.GetDocument(url, null);

            if (!string.IsNullOrEmpty(document))
            {
                UpdateInfoTemplate template = JsonHelper.Deserialize<UpdateInfoTemplate>(document);
                return new UpdateInfo(template);
            }

            return null;
        }

        /// <summary>
        /// Gets the download link of a specific update step.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="step">The update step to retrieve the download link of.</param>
        /// <returns>Returns a link that can be used to download the update step.</returns>
        private static string GetUpdateLink(string username, string token, UpdateStep step)
        {
            const string win64Package = "core-win64";
            const string win32Package = "core-win32";
            string package = Environment.Is64BitOperatingSystem ? win64Package : win32Package;

            string url = $"{BaseUrl}/get-download-link?username={username}&token={token}&apiVersion={ApiVersion}&package={package}&from={step.From}&to={step.To}";
            string document = WebHelper.GetDocument(url, null);

            int firstIndex = document.IndexOf('"');
            int lastIndex = document.LastIndexOf('"');
            string link = document.Substring(firstIndex + 1, lastIndex - firstIndex - 1);

            return link;
        }

        /// <summary>
        /// Downloads an update step into the specified file.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <param name="step">The update step to download.</param>
        /// <param name="destinationPath">The path to the file the update step will be downloaded to.</param>
        /// <param name="progress">A progress object used to report the progress of the operation.</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the operation.</param>
        /// <returns>Returns the downloaded file.</returns>
        public static async Task<FileInfo> DownloadUpdateStepAsync(string username, string token, UpdateStep step, string destinationPath,
            IProgress<double> progress, CancellationToken cancellationToken)
        {
            string url = GetUpdateLink(username, token, step);

            var file = new FileInfo(destinationPath);
            await WebHelper.DownloadFileAsync(new Uri(url), null, file, progress, cancellationToken);

            return cancellationToken.IsCancellationRequested ? null : file;
        }
    }
}
