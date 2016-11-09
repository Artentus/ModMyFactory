using System;

namespace ModMyFactory.Win32
{
    /// <summary>
    /// Standard rights. Contains the object's standard access rights.
    /// </summary>
    [Flags]
    enum StandardAccessRights : uint
    {
        /// <summary>
        /// Delete access.
        /// </summary>
        Delete = 0x00010000,

        /// <summary>
        /// Read access to the owner, group, and discretionary access control list (DACL) of the security descriptor.
        /// </summary>
        ReadControl = 0x00020000,

        /// <summary>
        /// Write access to the discretionary access control list (DACL).
        /// </summary>
        WriteDAC = 0x00040000,

        /// <summary>
        /// Write access to owner.
        /// </summary>
        WriteOwner = 0x00080000,

        /// <summary>
        /// Synchronize access.
        /// </summary>
        Synchronize = 0x00100000,


        /// <summary>
        /// Read access.
        /// </summary>
        Read = ReadControl,

        /// <summary>
        /// Write access.
        /// </summary>
        Write = ReadControl,

        /// <summary>
        /// Execute access.
        /// </summary>
        Execute = ReadControl,


        /// <summary>
        /// Delete, ReadControl, WriteDAC and WriteOwner.
        /// </summary>
        Required = Delete | ReadControl | WriteDAC | WriteOwner,

        /// <summary>
        /// Delete, ReadControl, WriteDAC, WriteOwner and Synchronize.
        /// </summary>
        All = Delete | ReadControl | WriteDAC | WriteOwner | Synchronize,
    }
}
