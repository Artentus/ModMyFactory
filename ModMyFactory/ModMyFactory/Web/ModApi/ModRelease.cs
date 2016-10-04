using System;
using System.ComponentModel;
using ModMyFactory.MVVM;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ModRelease : NotifyPropertyChangedBase
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        [JsonProperty("factorio_version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version FactorioVersion { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("downloads_count")]
        public int DownloadCount { get; set; }

        bool isInstalled;

        public bool IsInstalled
        {
            get { return isInstalled; }
            set
            {
                if (value != isInstalled)
                {
                    isInstalled = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsInstalled)));
                }
            }
        }
    }
}
