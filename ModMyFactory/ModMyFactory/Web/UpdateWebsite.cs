using ModMyFactory.Helpers;
using ModMyFactory.Web.UpdateApi;

namespace ModMyFactory.Web
{
    /// <summary>
    /// Represents the updater.factorio.com website.
    /// </summary>
    static class UpdateWebsite
    {
        const string UpdateUrl = "https://updater.factorio.com/get-available-versions";

        /// <summary>
        /// Get all available Factorio updates.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="token">The login token.</param>
        /// <returns>Returns information about all available updates for Factorio.</returns>
        public static UpdateInfo GetUpdateInfo(string username, string token)
        {
            string document = WebHelper.GetDocument($"{UpdateUrl}?username={username}&token={token}&apiVersion=2", null);

            if (!string.IsNullOrEmpty(document))
            {
                UpdateInfo result = JsonHelper.Deserialize<UpdateInfo>(document);
                return result;
            }

            return null;
        }
    }
}
