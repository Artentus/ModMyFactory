namespace ModMyFactory.FactorioUpdate
{
    /// <summary>
    /// Specifies the type of error the Factorio updater encounters.
    /// </summary>
    enum UpdaterErrorType
    {
        /// <summary>
        /// The provided archive is not a valid update package.
        /// </summary>
        PackageInvalid,

        /// <summary>
        /// The updater could not find a file specified in the update package.
        /// </summary>
        FileNotFound,

        /// <summary>
        /// A file has a different CRC32 checksum than provided by the update package.
        /// </summary>
        ChecksumMismatch,

        /// <summary>
        /// The resulting Factorio installation is corrupt.
        /// </summary>
        InstallationCorrupt,
    }
}
