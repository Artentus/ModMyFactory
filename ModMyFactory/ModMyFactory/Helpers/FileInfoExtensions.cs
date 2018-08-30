using System.IO;
using System.Threading.Tasks;
using ModMyFactory.Zlib;

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

        /// <summary>
        /// Calculates the CRC checksum of the file.
        /// </summary>
        public static uint CalculateCrc(this FileInfo file)
        {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                return Crc32Checksum.Generate(stream);
            }
        }

        /// <summary>
        /// Moves the file to a new location.
        /// </summary>
        public static async Task MoveToAsync(this FileInfo file, string destination)
        {
            await Task.Run(() => file.MoveTo(destination));
        }
    }
}
