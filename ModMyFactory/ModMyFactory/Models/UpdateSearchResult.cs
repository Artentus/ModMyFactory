namespace ModMyFactory.Models
{
    /// <summary>
    /// The result of an update search.
    /// </summary>
    class UpdateSearchResult
    {
        /// <summary>
        /// Indicates whether an update is available.
        /// </summary>
        public bool UpdateAvailable { get; }

        /// <summary>
        /// The url of the update.
        /// </summary>
        public string UpdateUrl { get; }

        /// <summary>
        /// The asset url of the update.
        /// </summary>
        public string AssetUrl { get; }

        /// <summary>
        /// The version of the update.
        /// </summary>
        public ExtendedVersion Version { get; }

        /// <summary>
        /// Creates a new update search result.
        /// </summary>
        /// <param name="updateAvailable">Indicates whether an update is available.</param>
        /// <param name="updateUrl">The url of the update.</param>
        /// <param name="assetUrl">The asset url of the update.</param>
        /// <param name="version">The version of the update.</param>
        public UpdateSearchResult(bool updateAvailable, string updateUrl, string assetUrl, ExtendedVersion version)
        {
            UpdateAvailable = updateAvailable;
            UpdateUrl = updateUrl;
            AssetUrl = assetUrl;
            Version = version;
        }
    }
}
