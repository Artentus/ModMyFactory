using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    /// <summary>
    /// A mod.
    /// </summary>
    class Mod : NotifyPropertyChangedBase
    {
        bool active;

        /// <summary>
        /// The name of the mod.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The description of the mod.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The author of the mod.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// The version of the mod.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The mod file.
        /// </summary>
        public FileInfo File { get; }

        /// <summary>
        /// The version of Factorio this mod is compatible with.
        /// </summary>
        public Version FactorioVersion { get; }

        /// <summary>
        /// Indicates whether the mod is currently active.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (value != active)
                {
                    active = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Active)));
                }
            }
        }

        /// <summary>
        /// Creates a mod.
        /// </summary>
        /// <param name="file">The mod file.</param>
        /// <param name="factorioVersion">The version of Factorio this mod is compatible with.</param>
        public Mod(FileInfo file, Version factorioVersion)
        {
            File = file;
            FactorioVersion = factorioVersion;

            using (ZipArchive archive = ZipFile.OpenRead(File.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("info.json"))
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                string content = reader.ReadToEnd();

                                // Name
                                MatchCollection matches = Regex.Matches(content, "\"title\" *: *\"(?<title>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    Name = matches[0].Groups["title"].Value;
                                }
                                else
                                {
                                    matches = Regex.Matches(content, "\"name\" *: *\"(?<name>.*)\"",
                                        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                    if (matches.Count > 0)
                                    {
                                        Name = matches[0].Groups["name"].Value;
                                    }
                                }

                                // Description
                                matches = Regex.Matches(content, "\"description\" *: *\"(?<description>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    Description = matches[0].Groups["description"].Value;
                                }

                                // Author
                                matches = Regex.Matches(content, "\"author\" *: *\"(?<author>.*)\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    Author = matches[0].Groups["author"].Value;
                                }

                                // Version
                                matches = Regex.Matches(content, "\"version\" *: *\"(?<version>[0-9]+(\\.[0-9]+){0,3})\"",
                                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                                if (matches.Count > 0)
                                {
                                    string versionString = matches[0].Groups["version"].Value;
                                    Version = Version.Parse(versionString);
                                }
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(Name)) Name = File.Name;
            if (Version == null) Version = new Version(1, 0);
        }
    }
}
