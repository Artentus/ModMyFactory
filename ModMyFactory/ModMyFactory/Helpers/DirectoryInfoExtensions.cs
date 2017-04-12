using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactory.Helpers
{
    static class DirectoryInfoExtensions
    {
        private static void MoveDirectoryRecursiveInternal(DirectoryInfo source, string destination, bool onSameVolume)
        {
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

            foreach (var file in source.EnumerateFiles())
                file.MoveTo(Path.Combine(destination, file.Name));

            foreach (var directory in source.EnumerateDirectories())
                MoveDirectoryInternal(directory, Path.Combine(destination, directory.Name), onSameVolume);

            source.Delete(false);
        }

        private static void MoveDirectoryInternal(DirectoryInfo source, string destination, bool onSameVolume)
        {
            if (onSameVolume && !Directory.Exists(destination))
            {
                source.MoveTo(destination);
            }
            else
            {
                MoveDirectoryRecursiveInternal(source, destination, onSameVolume);
            }
        }

        /// <summary>
        /// Moves this directory to a new path.
        /// </summary>
        /// <param name="source">The source directory.</param>
        /// <param name="destination">The destination path to move the directory to. This path can point to a different volume.</param>
        public static async Task MoveToAsync(this DirectoryInfo source, string destination)
        {
            destination = Path.GetFullPath(destination);
            bool onSameVolume = string.Equals(source.Root.Name, Path.GetPathRoot(destination), StringComparison.InvariantCultureIgnoreCase);
            await Task.Run(() => MoveDirectoryInternal(source, destination, onSameVolume));
        }

        private static void CopyDirectoryRecursiveInternal(DirectoryInfo source, string destination)
        {
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(destination, file.Name));

            foreach (var directory in source.GetDirectories())
                CopyDirectoryRecursiveInternal(directory, Path.Combine(destination, directory.Name));
        }

        /// <summary>
        /// Copies this directory to a new path.
        /// </summary>
        /// <param name="source">The source directory.</param>
        /// <param name="destination">The destination path to copy the directory to. This path can point to a different volume.</param>
        public static async Task CopyToAsync(this DirectoryInfo source, string destination)
        {
            destination = Path.GetFullPath(destination);
            await Task.Run(() => CopyDirectoryRecursiveInternal(source, destination));
        }

        /// <summary>
        /// Checks if two DirectoryInfo objects point to the same directory.
        /// </summary>
        public static bool DirectoryEquals(this DirectoryInfo first, DirectoryInfo second)
        {
            string firstPath = first.FullName.TrimEnd('\\');
            string secondPath = second.FullName.TrimEnd('\\');
            return string.Equals(firstPath, secondPath, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Checks if this directory is empty.
        /// </summary>
        /// <returns>Returns true if the directory is empty, otherwise false.</returns>
        public static bool IsEmpty(this DirectoryInfo directory)
        {
            return !directory.EnumerateFileSystemInfos().Any();
        }

        /// <summary>
        /// Deletes this directory if it is empty.
        /// </summary>
        public static void DeleteIfEmpty(this DirectoryInfo directory)
        {
            if (directory.IsEmpty()) directory.Delete();
        }
    }
}
