using ModMyFactory.Helpers;
using ModMyFactory.ModSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ModMyFactory.Models
{
    /// <summary>
    /// Represents a mod file or directory.
    /// </summary>
    sealed class ModFile : IComparable<ModFile>
    {
        private const string DefaultLocaleString = "en";

        private readonly bool isFile;
        private FileSystemInfo file;
        private Dictionary<string, ModLocale> locales;
        private ModSettingInfo[] settings;
        
        /// <summary>
        /// The mods info file.
        /// </summary>
        public InfoFile InfoFile { get; }

        /// <summary>
        /// The mods name.
        /// </summary>
        public string Name => InfoFile.Name;

        /// <summary>
        /// The mods version.
        /// </summary>
        public GameCompatibleVersion Version => InfoFile.Version;

        /// <summary>
        /// Indicates whether this mod file is enabled.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Indicates whether updates for this mod should be extracted.
        /// </summary>
        public bool ExtractUpdates => !(isFile || App.Instance.Settings.AlwaysUpdateZipped);
        
        /// <summary>
        /// Indicaes whether this mod file resides inside the managed mod directory.
        /// </summary>
        public bool ResidesInModDirectory => file.ParentDirectory().DirectoryEquals(App.Instance.Settings.GetModDirectory(InfoFile.FactorioVersion));

        /// <summary>
        /// An optional thumbnail provided in the mod file.
        /// </summary>
        public BitmapImage Thumbnail { get; }

        private string BuildNewFileName(int uid, bool enabled)
        {
            var sb = new StringBuilder();

            if (uid >= 0)
            {
                sb.Append(uid);
                sb.Append('+');
            }

            sb.Append(Name);
            sb.Append('_');
            sb.Append(Version);

            if (isFile)
            {
                if (enabled) sb.Append(".zip");
                else sb.Append(".disabled");
            }

            return sb.ToString();
        }

        private FileSystemInfo GetNewFile(string newPath)
        {
            return isFile ? (FileSystemInfo)(new FileInfo(newPath)) : (FileSystemInfo)(new DirectoryInfo(newPath));
        }

        /// <summary>
        /// Moves this mod file to a new location.
        /// </summary>
        /// <param name="destination">The location to move the file to.</param>
        /// <param name="uid">Optional. A UID to append to the file name.</param>
        public async Task MoveToAsync(string destination, int uid = -1)
        {
            string newName = BuildNewFileName(uid, Enabled);
            string newPath = Path.Combine(destination, newName);
            await file.MoveToAsync(newPath);

            file = GetNewFile(newPath);
        }

        /// <summary>
        /// Copies this mod file to a specified location.
        /// </summary>
        /// <param name="destination">The location to copy the file to.</param>
        /// <param name="uid">Optional. A UID to append to the file name.</param>
        /// <returns>Returns a new mod file object representing the copy created.</returns>
        public async Task<ModFile> CopyToAsync(string destination, int uid = -1)
        {
            string newName = BuildNewFileName(uid, true);
            string newPath = Path.Combine(destination, newName);
            await file.CopyToAsync(newPath);

            var newFile = GetNewFile(newPath);
            return new ModFile(newFile, InfoFile, isFile, true, Thumbnail);
        }

        /// <summary>
        /// Extracts this mod file to the same location and deletes the original.
        /// If the file is already extracted no action is taken.
        /// </summary>
        /// <returns>Returns the extracted mod file.</returns>
        public async Task<ModFile> ExtractAsync()
        {
            if (!isFile) return this;

            var fi = (FileInfo)file;
            await Task.Run(() => ZipFile.ExtractToDirectory(fi.FullName, fi.DirectoryName));

            var newDir = new DirectoryInfo(Path.Combine(fi.DirectoryName, fi.NameWithoutExtension()));
            var newModFile = new ModFile(newDir, InfoFile, false, Enabled, Thumbnail);

            fi.Delete();
            return newModFile;
        }

        /// <summary>
        /// Deletes this mod file.
        /// </summary>
        public void Delete()
        {
            file.DeleteRecursive();
        }

        public int CompareTo(ModFile other)
        {
            int result = Version.CompareTo(other.Version);
            
            if (result == 0)
            {
                if (isFile)
                {
                    result = other.isFile ? 0 : -1;
                }
                else
                {
                    result = other.isFile ? 1 : 0;
                }
            }

            return result;
        }

        private ModFile(FileSystemInfo file, InfoFile infoFile, bool isFile, bool enabled, BitmapImage thumbnail)
        {
            this.file = file;
            InfoFile = infoFile;
            this.isFile = isFile;
            Enabled = enabled;
            Thumbnail = thumbnail;
        }

        /// <summary>
        /// Enables this mod file.
        /// </summary>
        public void Enable()
        {
            if (Enabled) return;

            if (isFile)
            {
                string newName = BuildNewFileName(-1, true);
                string newPath = Path.Combine(file.ParentDirectory().FullName, newName);
                file.MoveTo(newPath);
                file = GetNewFile(newPath);
            }
            else
            {
                string infoPath = Path.Combine(file.FullName, "info.disabled");
                var infoFile = new FileInfo(infoPath);
                if (infoFile.Exists) infoFile.MoveTo(Path.Combine(file.FullName, "info.json"));
            }

            Enabled = true;
        }

        /// <summary>
        /// Disables this mod file.
        /// </summary>
        public void Disable()
        {
            if (!Enabled) return;

            if (isFile)
            {
                string newName = BuildNewFileName(-1, false);
                string newPath = Path.Combine(file.ParentDirectory().FullName, newName);
                file.MoveTo(newPath);
                file = GetNewFile(newPath);
            }
            else
            {
                string infoPath = Path.Combine(file.FullName, "info.json");
                var infoFile = new FileInfo(infoPath);
                if (infoFile.Exists) infoFile.MoveTo(Path.Combine(file.FullName, "info.disabled"));
            }

            Enabled = false;
        }

        /// <summary>
        /// Sets the files enabled state.
        /// </summary>
        public void SetEndabled(bool value)
        {
            if (value) Enable();
            else Disable();
        }
        
        public ModSettingInfo[] GetSettings()
        {
            // ToDo: read settings using Lua interpreter

            if (settings == null) settings = new ModSettingInfo[0];
            return settings;
        }

        private static bool TryLoadLocaleFromFile(FileInfo archiveFile, CultureInfo culture, out ModLocale locale)
        {
            using (var archive = ZipFile.OpenRead(archiveFile.FullName))
            {
                string localeDir = "/locale/" + culture.TwoLetterISOLanguageName;

                var streamList = new List<Stream>();
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.Contains(localeDir) && (entry.FullName.Count(c => c == '/') == 3) && entry.Name.EndsWith(".cfg"))
                    {
                        var stream = entry.Open();
                        streamList.Add(stream);
                    }
                }

                if (streamList.Count == 0)
                {
                    locale = null;
                    return false;
                }
                else
                {
                    try
                    {
                        locale = new ModLocale(culture, streamList);
                        return true;
                    }
                    finally
                    {
                        streamList?.ForEach(stream => stream?.Close());
                    }
                }
            }
        }

        private static bool TryLoadLocaleFromDirectory(DirectoryInfo directory, CultureInfo culture, out ModLocale locale)
        {
            var localeParentDir = new DirectoryInfo(Path.Combine(directory.FullName, "locale"));
            var localeDir = localeParentDir.EnumerateDirectories().FirstOrDefault(dir => dir.Name.StartsWith(culture.TwoLetterISOLanguageName));
            if (localeDir == null)
            {
                locale = null;
                return false;
            }

            var files = localeDir.EnumerateFiles("*.cfg");
            if (files.Any())
            {
                locale = new ModLocale(culture, files);
                return true;
            }
            else
            {
                locale = null;
                return false;
            }
        }

        private bool TryLoadLocale(CultureInfo culture, out ModLocale locale)
        {
            if (isFile)
            {
                return TryLoadLocaleFromFile((FileInfo)file, culture, out locale);
            }
            else
            {
                return TryLoadLocaleFromDirectory((DirectoryInfo)file, culture, out locale);
            }
        }

        private ModLocale GetDefaultLocale()
        {
            
            if (locales.TryGetValue(DefaultLocaleString, out var storedValue))
            {
                return storedValue;
            }
            else
            {
                var culture = new CultureInfo(DefaultLocaleString);
                if (!TryLoadLocale(culture, out var locale))
                    locale = new ModLocale(culture);

                locales.Add(culture.TwoLetterISOLanguageName, locale);
                return locale;
            }
        }

        public ModLocale GetLocale(CultureInfo culture)
        {
            if (locales == null) locales = new Dictionary<string, ModLocale>();

            if (culture.TwoLetterISOLanguageName == DefaultLocaleString)
                return GetDefaultLocale();

            if (locales.TryGetValue(culture.TwoLetterISOLanguageName, out var storedValue))
            {
                return storedValue;
            }
            else
            {
                if (!TryLoadLocale(culture, out var locale))
                    locale = GetDefaultLocale();

                locales.Add(culture.TwoLetterISOLanguageName, locale);
                return locale;
            }
        }

        private static BitmapImage LoadImageFromStream(Stream stream)
        {
            var result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }

        private static BitmapImage GetThumbnailFromArchive(FileInfo archiveFile)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(archiveFile.FullName))
                {
                    var entry = archive.Entries.FirstOrDefault(e => (e.Name == "thumbnail.png") && (e.FullName.Count(c => c == '/') == 1));
                    if (entry == null) return null;

                    using (var stream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            var buffer = new byte[8192];
                            int count = 0;
                            do
                            {
                                count = stream.Read(buffer, 0, buffer.Length);
                                if (count > 0) ms.Write(buffer, 0, count);
                            } while (count > 0);

                            return LoadImageFromStream(ms);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return null;
            }
        }

        private static BitmapImage GetThumbnailFromDirectory(DirectoryInfo directory)
        {
            try
            {
                var file = directory.EnumerateFiles("thumbnail.png").FirstOrDefault();
                if (file == null) return null;

                using (var stream = file.OpenRead())
                    return LoadImageFromStream(stream);
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return null;
            }
        }

        /// <summary>
        /// Tries to load a file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <param name="result">Out. The loaded mod file.</param>
        /// <param name="hasUid">Optional. Specifies if the file to load contains a UID in its name.</param>
        /// <returns>Returns true if the specified file is a valid mod file, otherwise false.</returns>
        public static bool TryLoadFromFile(FileInfo file, out ModFile result, bool hasUid = false)
        {
            result = null;
            if (!file.Exists) return false;

            InfoFile infoFile;
            bool enabled;
            if (!ArchiveFileValid(file, out infoFile, out enabled, hasUid)) return false;

            var thumbnail = GetThumbnailFromArchive(file);
            result = new ModFile(file, infoFile, true, enabled, thumbnail);
            return true;
        }

        /// <summary>
        /// Tries to load a directory.
        /// </summary>
        /// <param name="directory">The directory to load.</param>
        /// <param name="result">Out. The loaded mod file.</param>
        /// <param name="hasUid">Optional. Specifies if the directory to load contains a UID in its name.</param>
        /// <returns>Returns true if the specified directory is a valid mod directory, otherwise false.</returns>
        public static bool TryLoadFromDirectory(DirectoryInfo directory, out ModFile result, bool hasUid = false)
        {
            result = null;
            if (!directory.Exists) return false;

            InfoFile infoFile;
            bool enabled;
            if (!DirectoryValid(directory, out infoFile, out enabled, hasUid)) return false;

            var thumbnail = GetThumbnailFromDirectory(directory);
            result = new ModFile(directory, infoFile, false, enabled, thumbnail);
            return true;
        }

        /// <summary>
        /// Tries to load a file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <param name="result">Out. The loaded mod file.</param>
        /// <param name="hasUid">Optional. Specifies if the file to load contains a UID in its name.</param>
        /// <returns>Returns true if the specified file is a valid mod file, otherwise false.</returns>
        public static bool TryLoad(FileSystemInfo file, out ModFile result, bool hasUid = false)
        {
            result = null;
            if (!file.Exists) return false;

            var fi = file as FileInfo;
            if (fi != null) return TryLoadFromFile(fi, out result, hasUid);

            var di = file as DirectoryInfo;
            if (di != null) return TryLoadFromDirectory(di, out result, hasUid);

            return false;
        }
        
        /// <summary>
        /// Removes the UID from a mods name, if it is specified.
        /// </summary>
        /// <param name="name">The mods name, optionally containing a UID.</param>
        /// <returns>Returns the mods name without UID.</returns>
        private static string GetNameWithoutUid(string name)
        {
            int index = name.IndexOf('+');
            if (index < 0) return name;

            return name.Substring(index + 1);
        }

        /// <summary>
        /// Tries to read mod name and mod version from a file name.
        /// The file name can not contain an extension.
        /// </summary>
        /// <param name="fileName">The file name to parse.</param>
        /// <param name="extension">The files extension or null.</param>
        /// <param name="name">Out. The parsed mod name.</param>
        /// <param name="version">Out. The parsed mod version.</param>
        /// <param name="enabled">Out. Indicates if the mod file is enabled.</param>
        /// <param name="hasUid">Specifies if the file name contains a UID.</param>
        /// <returns>Returns true if the file name was a valid mod file name, otherwise false.</returns>
        private static bool TryParseModName(string fileName, string extension, out string name, out GameCompatibleVersion version, out bool enabled, bool hasUid)
        {
            name = null;
            version = null;

            enabled = (extension != ".disabled");
            
            int index = fileName.LastIndexOf('_');
            if ((index < 1) || (index >= fileName.Length - 1)) return false;

            name = fileName.Substring(0, index);
            if (hasUid) name = GetNameWithoutUid(name);
            string versionString = fileName.Substring(index + 1);
            return GameCompatibleVersion.TryParse(versionString, out version);
        }

        /// <summary>
        /// Tries to deserialize a given stream into an info file object.
        /// </summary>
        /// <param name="stream">The stream to deserialize.</param>
        /// <param name="infoFile">Out. The deserialized info file if the operation was successful.</param>
        /// <returns>Returns true if deserialization succeeded, otherwise false.</returns>
        private static bool TryDeserializeInfoFileFromStream(Stream stream, out InfoFile infoFile)
        {
            try
            {
                infoFile = InfoFile.FromJsonStream(stream);
                return infoFile.IsValid;
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);

                infoFile = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to read an info file from an archive.
        /// </summary>
        /// <param name="archiveFile">The archive to read from.</param>
        /// <param name="infoFile">Out. The info file that has been read.</param>
        /// <returns>Returns true if an info file could be read from the archive, otherwise false.</returns>
        private static bool TryReadInfoFileFromArchive(FileInfo archiveFile, out InfoFile infoFile)
        {
            infoFile = null;

            try
            {
                using (var archive = ZipFile.OpenRead(archiveFile.FullName))
                {
                    var entry = archive.Entries.FirstOrDefault(e => (e.Name == "info.json") && (e.FullName.Count(c => c == '/') == 1));
                    if (entry == null) return false;

                    using (var stream = entry.Open())
                        return TryDeserializeInfoFileFromStream(stream, out infoFile) && infoFile.IsValid;
                }
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Tries to read an info file from a directory.
        /// </summary>
        /// <param name="directory">The directory to read fromn.</param>
        /// <param name="infoFile">Out. The info file that has been read.</param>
        /// <returns>Returns true if an info file could be read from the directory, otherwise false.</returns>
        private static bool TryReadInfoFileFromDirectory(DirectoryInfo directory, out InfoFile infoFile, out bool enabled)
        {
            infoFile = null;
            enabled = true;

            try
            {
                string enabledPath = Path.Combine(directory.FullName, "info.json");
                string disabledPath = Path.Combine(directory.FullName, "info.disabled");

                var file = new FileInfo(enabledPath);
                if (!file.Exists)
                {
                    enabled = false;
                    file = new FileInfo(disabledPath);
                    if (!file.Exists) return false;
                }

                using (var stream = file.OpenRead())
                    return TryDeserializeInfoFileFromStream(stream, out infoFile) && infoFile.IsValid;
            }
            catch (Exception ex)
            {
                App.Instance.WriteExceptionLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Determines if an archive file is a valid mod.
        /// </summary>
        /// <param name="archiveFile">The archive file to check.</param>
        /// <param name="infoFile">Out. The mods info file.</param>
        /// <param name="hasUid">Specifies if the file name contains a UID.</param>
        /// <returns>Returns true if the specified file is a valid mod, otherwise false.</returns>
        private static bool ArchiveFileValid(FileInfo archiveFile, out InfoFile infoFile, out bool enabled, bool hasUid)
        {
            infoFile = null;

            string fileName;
            GameCompatibleVersion fileVersion;
            if (!TryParseModName(archiveFile.NameWithoutExtension(), archiveFile.Extension, out fileName, out fileVersion, out enabled, hasUid)) return false;

            if (TryReadInfoFileFromArchive(archiveFile, out infoFile))
            {
                return (infoFile.Name == fileName) && (infoFile.Version == fileVersion);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if a directory is a valid mod.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <param name="infoFile">Out. The mods info file.</param>
        /// <param name="hasUid">Specifies if the directory name contains a UID.</param>
        /// <returns>Returns true if the specified directory is a valid mod, otherwise false.</returns>
        private static bool DirectoryValid(DirectoryInfo directory, out InfoFile infoFile, out bool enabled, bool hasUid)
        {
            infoFile = null;

            if (directory.Name == "base")
            {
                enabled = true;
                return TryReadInfoFileFromDirectory(directory, out infoFile, out enabled);
            }
            else
            {
                string fileName;
                GameCompatibleVersion fileVersion;
                if (!TryParseModName(directory.Name, string.Empty, out fileName, out fileVersion, out enabled, hasUid)) return false;

                if (TryReadInfoFileFromDirectory(directory, out infoFile, out enabled))
                {
                    return (infoFile.Name == fileName) && (infoFile.Version == fileVersion);
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
