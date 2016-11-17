using System;

namespace ModMyFactory.FactorioUpdate
{
    /// <summary>
    /// This exception is thrown if the Factorio updater encounters a critical error.
    /// </summary>
    class CriticalUpdaterException : Exception
    {
        /// <summary>
        /// The type of error the updater encountered.
        /// </summary>
        public UpdaterErrorType ErrorType { get; }

        public CriticalUpdaterException(UpdaterErrorType errorType, Exception innerException)
            : base("The updater encountered a critical error.", innerException)
        {
            ErrorType = errorType;
        }

        public CriticalUpdaterException(UpdaterErrorType errorType)
            : this(errorType, null)
        { }
    }
}
