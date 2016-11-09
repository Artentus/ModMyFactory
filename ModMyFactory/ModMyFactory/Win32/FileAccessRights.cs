using System;

namespace ModMyFactory.Win32
{
    [Flags]
    enum FileAccessRights : uint
    {
        /*
         * Directory
         */
        
        /// <summary>
        /// For a directory, the right to create a file in the directory.
        /// </summary>
        AddFile = 0x00000002,

        /// <summary>
        /// For a directory, the right to create a subdirectory.
        /// </summary>
        AddSubdirectory = 0x00000004,

        /// <summary>
        /// For a directory, the right to delete a directory and all the files it contains, including read-only files.
        /// </summary>
        DeleteChild = 0x00000040,

        /// <summary>
        /// For a directory, the right to list the contents of the directory.
        /// </summary>
        ListDirectory = 0x00000001,

        /// <summary>
        /// For a directory, the right to traverse the directory.
        /// By default, users are assigned the BYPASS_TRAVERSE_CHECKING privilege, which ignores FileAccessRights.Traverse.
        /// </summary>
        Traverse = 0x00000020,


        /*
         * File
         */
        
        /// <summary>
        /// For a file, the right to append data to the file.
        /// For local files, write operations will not overwrite existing data if this flag is specified without FileAccessRights.WriteData.
        /// </summary>
        AppendData = 0x00000004,

        /// <summary>
        /// For a native code file, the right to execute the file.
        /// This access right given to scripts may cause the script to be executable, depending on the script interpreter.
        /// </summary>
        Execute = 0x00000020,


        /*
         * File and directory
         */
        
        /// <summary>
        /// The right to read extended file or directory attributes.
        /// </summary>
        ReadExtendedAttributes = 0x00000008,

        /// <summary>
        /// The right to write extended file or directory attributes.
        /// </summary>
        WriteExtendedAttributes = 0x00000010,


        /*
         * Pipe
         */
        
        /// <summary>
        /// For a named pipe, the right to create a pipe.
        /// </summary>
        CreatePipeInstance = 0x00000004,


        /*
         * File and pipe
         */
        
        /// <summary>
        /// For a file or named pipe, the right to read the corresponding file or pipe data.
        /// </summary>
        ReadData = 0x00000001,

        /// <summary>
        /// For a file or named pipe, the right to write data to the file or pipe.
        /// </summary>
        WriteData = 0x00000002,


        /*
         * All
         */
        
        /// <summary>
        /// The right to read attributes.
        /// </summary>
        ReadAttributes = 0x00000080,

        /// <summary>
        /// The right to write attributes.
        /// </summary>
        WriteAttributes = 0x00000100,



        /*
         * Generic
         */
        
        GenericRead = ReadData | ReadAttributes | ReadExtendedAttributes | StandardAccessRights.Read | StandardAccessRights.Synchronize,
        GenericWrite = WriteData | AppendData | WriteAttributes | WriteExtendedAttributes | StandardAccessRights.Write | StandardAccessRights.Synchronize,
        GenericExecute = Execute | ReadAttributes | StandardAccessRights.Execute | StandardAccessRights.Synchronize,

        GenericAll = 0x000001FF | StandardAccessRights.Required | StandardAccessRights.Synchronize,
    }
}
