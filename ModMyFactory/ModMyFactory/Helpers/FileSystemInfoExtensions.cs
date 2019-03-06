using System.IO;
using System.Threading.Tasks;

namespace ModMyFactory.Helpers
{
    static class FileSystemInfoExtensions
    {
        /// <summary>
        /// Gets the name of the file without the extension.
        /// </summary>
        public static string NameWithoutExtension(this FileSystemInfo file)
        {
            FileInfo fileInfo = file as FileInfo;
            if (fileInfo != null) return fileInfo.NameWithoutExtension();
            else return file.Name;
        }

        /// <summary>
        /// Deletes this file system object. Makes sure directories with content are deleted recursively.
        /// </summary>
        public static void DeleteRecursive(this FileSystemInfo file)
        {
            FileInfo fileInfo = file as FileInfo;
            if (fileInfo != null)
            {
                fileInfo.Delete();
                return;
            }

            DirectoryInfo directoryInfo = file as DirectoryInfo;
            if (directoryInfo != null)
            {
                directoryInfo.Delete(true);
                return;
            }

            file.Delete();
        }

        /// <summary>
        /// Moves the file to a new location.
        /// </summary>
        public static async Task MoveToAsync(this FileSystemInfo file, string destination)
        {
            FileInfo fileInfo = file as FileInfo;
            if (fileInfo != null)
            {
                await fileInfo.MoveToAsync(destination);
                return;
            }

            DirectoryInfo directoryInfo = file as DirectoryInfo;
            if (directoryInfo != null)
            {
                await directoryInfo.MoveToAsync(destination);
                return;
            }
        }

        /// <summary>
        /// Moves the file to a new location.
        /// </summary>
        public static void MoveTo(this FileSystemInfo file, string destination)
        {
            FileInfo fileInfo = file as FileInfo;
            if (fileInfo != null)
            {
                fileInfo.MoveTo(destination);
                return;
            }

            DirectoryInfo directoryInfo = file as DirectoryInfo;
            if (directoryInfo != null)
            {
                directoryInfo.MoveTo(destination);
                return;
            }
        }

        /// <summary>
        /// Copies the file to a new location.
        /// </summary>
        public static async Task CopyToAsync(this FileSystemInfo file, string destination)
        {
            FileInfo fileInfo = file as FileInfo;
            if (fileInfo != null)
            {
                await Task.Run(() => fileInfo.CopyTo(destination));
                return;
            }

            DirectoryInfo directoryInfo = file as DirectoryInfo;
            if (directoryInfo != null)
            {
                await directoryInfo.CopyToAsync(destination);
                return;
            }
        }

        /// <summary>
        /// Gets the files parent directory.
        /// </summary>
        public static DirectoryInfo ParentDirectory(this FileSystemInfo file)
        {
            FileInfo fileInfo = file as FileInfo;
            if (fileInfo != null) return fileInfo.Directory;

            DirectoryInfo directoryInfo = file as DirectoryInfo;
            if (directoryInfo != null) return directoryInfo.Parent;

            return new DirectoryInfo(Path.GetDirectoryName(file.FullName));
        }
    }
}
