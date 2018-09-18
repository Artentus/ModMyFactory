using System;
using System.IO;
using ModMyFactory.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class InfoFile
    {
        /// <summary>
        /// The unique name of the mod.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// The mods version.
        /// </summary>
        [JsonProperty("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; }

        /// <summary>
        /// The version of Factorio this mod is compatible with.
        /// </summary>
        [JsonProperty("factorio_version")]
        [JsonConverter(typeof(TwoPartVersionConverter))]
        public Version FactorioVersion { get; }

        /// <summary>
        /// The mods friendly name.
        /// </summary>
        [JsonProperty("title")]
        public string FriendlyName { get; }

        /// <summary>
        /// The author of the mod.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; }

        /// <summary>
        /// A description of the mod.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; }

        /// <summary>
        /// The mods dependencies.
        /// </summary>
        [JsonProperty("dependencies")]
        public ModDependency[] Dependencies { get; }

        /// <summary>
        /// Indicates whether this info file is valid.
        /// To be valid, the <see cref="Name"/>, <see cref="Version"/> and <see cref="FactorioVersion"/> properties must be non-null.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && (Version != null) && (FactorioVersion != null);

        [JsonConstructor]
        private InfoFile(string name, Version version, Version factorioVersion, string friendlyName, string author, string description, ModDependency[] dependencies)
        {
            Name = name;
            Version = version;
            FactorioVersion = factorioVersion;
            FriendlyName = friendlyName;
            Author = author;
            Description = description;
            Dependencies = dependencies;
        }

        /// <summary>
        /// Deserializes an info file from a JSON string.
        /// </summary>
        /// <param name="jsonString">The string to deserialize.</param>
        /// <returns>Returns the deserialized info file object.</returns>
        public static InfoFile FromJsonString(string jsonString)
        {
            return JsonHelper.Deserialize<InfoFile>(jsonString);
        }

        /// <summary>
        /// Deserializes an info file from a JSON stream.
        /// </summary>
        /// <param name="jsonStream">The stream to deserialize.</param>
        /// <returns>Returns the deserialized info file object.</returns>
        public static InfoFile FromJsonStream(Stream jsonStream)
        {
            using (var reader = new StreamReader(jsonStream))
            {
                string jsonString = reader.ReadToEnd();
                return FromJsonString(jsonString);
            }
        }
    }
}
