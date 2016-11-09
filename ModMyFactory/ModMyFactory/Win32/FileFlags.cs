using System;

namespace ModMyFactory.Win32
{
    [Flags]
    enum FileFlags : uint
    {
        None = 0,

        /// <summary>
        /// The file data is requested, but it should continue to be located in remote storage. It should not be transported back to local storage.
        /// This flag is for use by remote storage systems.
        /// </summary>
        OpenNoRecall = 0x00100000,

        /// <summary>
        /// Normal reparse point processing will not occur; CreateFile will attempt to open the reparse point. When a file is opened, a file handle is returned, whether or not the filter that controls the reparse point is operational.
        /// This flag cannot be used with FileMode.Create.
        /// If the file is not a reparse point, then this flag is ignored.
        /// </summary>
        OpenReparsePoint = 0x00200000,

        /// <summary>
        /// Access will occur according to POSIX rules. This includes allowing multiple files with names, differing only in case, for file systems that support that naming.
        /// Use care when using this option, because files created with this flag may not be accessible by applications that are written for MS-DOS or 16-bit Windows.
        /// </summary>
        PosixSemantics = 0x01000000,

        /// <summary>
        /// The file is being opened or created for a backup or restore operation. The system ensures that the calling process overrides file security checks when the process has SE_BACKUP_NAME and SE_RESTORE_NAME privileges.
        /// You must set this flag to obtain a handle to a directory. A directory handle can be passed to some functions instead of a file handle.
        /// </summary>
        BackupSemantics = 0x02000000,

        /// <summary>
        /// The file is to be deleted immediately after all of its handles are closed, which includes the specified handle and any other open or duplicated handles.
        /// If there are existing open handles to a file, the call fails unless they were all opened with FileShare.Delete.
        /// </summary>
        DeleteOnClose = 0x04000000,

        /// <summary>
        /// Access is intended to be sequential from beginning to end. The system can use this as a hint to optimize file caching.
        /// This flag should not be used if read-behind (that is, reverse scans) will be used.
        /// </summary>
        SequentialScan = 0x08000000,

        /// <summary>
        /// Access is intended to be random. The system can use this as a hint to optimize file caching.
        /// </summary>
        RandomAccess = 0x10000000,

        /// <summary>
        /// The file or device is being opened with no system caching for data reads and writes.
        /// This flag does not affect hard disk caching or memory mapped files.
        /// </summary>
        NoBuffering = 0x20000000,

        /// <summary>
        /// Write operations will not go through any intermediate cache, they will go directly to disk.
        /// </summary>
        WriteThrough = 0x80000000,
    }
}
