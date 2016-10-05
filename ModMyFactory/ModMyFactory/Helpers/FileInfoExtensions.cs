using System.IO;

namespace ModMyFactory.Helpers
{
    static class FileInfoExtensions
    {
        /// <summary>
        /// Gets the name of the file without the extension.
        /// </summary>
        public static string NameWithoutExtension(this FileInfo file)
        {
            return Path.GetFileNameWithoutExtension(file.Name);
        }
    }
}
