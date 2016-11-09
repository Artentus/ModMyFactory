using System;

namespace ModMyFactory.Win32
{
    /// <summary>
    /// You can use generic access rights to specify the type of access you need when you are opening a handle to an object.
    /// This is typically simpler than specifying all the corresponding standard and specific rights.
    /// </summary>
    [Flags]
    enum GenericAccessRights : uint
    {
        /// <summary>
        /// Read access.
        /// </summary>
        Read = 0x80000000,

        /// <summary>
        /// Write access.
        /// </summary>
        Write = 0x40000000,

        /// <summary>
        /// Execute access.
        /// </summary>
        Execute = 0x20000000,

        /// <summary>
        /// All possible access rights.
        /// </summary>
        All = 0x10000000,
    }
}
