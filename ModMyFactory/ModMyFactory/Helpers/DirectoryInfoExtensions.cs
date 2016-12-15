using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModMyFactory.Helpers
{
    static class DirectoryInfoExtensions
    {
        private static async Task MoveDirectoryRecursiveInnerAsync(DirectoryInfo source, DirectoryInfo destination)
        {
            if (!destination.Exists) destination.Create();

            await Task.Run(() =>
            {
                foreach (var file in source.GetFiles())
                    file.MoveTo(Path.Combine(destination.FullName, file.Name));
            });

            foreach (var directory in source.GetDirectories())
                await MoveDirectoryRecursiveInnerAsync(directory, new DirectoryInfo(Path.Combine(destination.FullName, directory.Name)));

            source.Delete(false);
        }

        /// <summary>
        /// Moves this directory to a new path.
        /// </summary>
        /// <param name="source">The source directory.</param>
        /// <param name="destination">The destination path to move the directory to. This path can point to a different volume.</param>
        public static async Task MoveToAsync(this DirectoryInfo source, string destination)
        {
            var destinationDirectory = new DirectoryInfo(destination);
            await MoveDirectoryRecursiveInnerAsync(source, destinationDirectory);
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
