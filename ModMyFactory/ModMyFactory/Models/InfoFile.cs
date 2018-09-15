using System;
using ModMyFactory.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Models
{
    partial class Mod
    {
        [JsonObject(MemberSerialization.OptIn)]
        protected sealed class InfoFile
        {
            [JsonProperty("name")]
            public string Name { get; }

            [JsonProperty("version")]
            [JsonConverter(typeof(VersionConverter))]
            public Version Version { get; }

            [JsonProperty("factorio_version")]
            [JsonConverter(typeof(TwoPartVersionConverter))]
            public Version FactorioVersion { get; }

            [JsonProperty("title")]
            public string Title { get; }

            [JsonProperty("author")]
            public string Author { get; }

            [JsonProperty("description")]
            public string Description { get; }

            [JsonProperty("dependencies")]
            [JsonConverter(typeof(ModDependencyJsonConverter))]
            public ModDependency[] Dependencies { get; }

            public bool IsValid => !string.IsNullOrWhiteSpace(Name) && (Version != null) && (FactorioVersion != null);

            [JsonConstructor]
            private InfoFile(string name, Version version, Version factorioVersion, string title, string author, string description, ModDependency[] dependencies)
            {
                Name = name;
                Version = version;
                FactorioVersion = factorioVersion;
                Title = title;
                Author = author;
                Description = description;
                Dependencies = dependencies;
            }

            public static InfoFile FromString(string value)
            {
                return JsonHelper.Deserialize<InfoFile>(value);
            }
        }
    }
}
