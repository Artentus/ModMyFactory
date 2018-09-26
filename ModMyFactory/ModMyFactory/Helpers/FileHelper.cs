using System.IO;

namespace ModMyFactory.Helpers
{
    static class FileHelper
    {
        /// <summary>
        /// Determines if a given path is pointing to a directory.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>Return true if the given path is pointing to a directory, returns false if it is pointing to a file.</returns>
        public static bool IsDirectory(string path)
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.Directory);
        }

        /// <summary>
        /// Creates a FileSystemInfo object from a given path.
        /// If the path is pointing to a directory a DirectoryInfo object is created, if it is pointing to a file a FileInfo object is created.
        /// </summary>
        /// <param name="path">The path of the file system object to create.</param>
        /// <returns>Returns a FIleSystemInfo object that was created from the given path.</returns>
        public static FileSystemInfo CreateFileOrDirectory(string path)
        {
            if (IsDirectory(path))
                return new DirectoryInfo(path);
            else
                return new FileInfo(path);
        }
    }
}
