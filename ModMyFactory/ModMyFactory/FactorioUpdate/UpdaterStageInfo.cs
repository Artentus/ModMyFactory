namespace ModMyFactory.FactorioUpdate
{
    /// <summary>
    /// Describes the stage of an updater operation.
    /// </summary>
    sealed class UpdaterStageInfo
    {
        /// <summary>
        /// Indicates whether this stage can be cancelled.
        /// </summary>
        public bool CanCancel { get; }

        /// <summary>
        /// A description of the stage.
        /// </summary>
        public string Description { get; }

        public UpdaterStageInfo(bool canCancel, string description)
        {
            CanCancel = canCancel;
            Description = description;
        }
    }
}
