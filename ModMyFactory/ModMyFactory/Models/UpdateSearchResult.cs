using System;

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
        /// The version of the update.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Creates a new update search result.
        /// </summary>
        /// <param name="updateAvailable">Indicates whether an update is available.</param>
        /// <param name="updateUrl">The url of the update.</param>
        /// <param name="version">The version of the update.</param>
        public UpdateSearchResult(bool updateAvailable, string updateUrl, Version version)
        {
            UpdateAvailable = updateAvailable;
            UpdateUrl = updateUrl;
            Version = version;
        }
    }
}
