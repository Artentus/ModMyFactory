using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WPFCore;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class ModRelease : NotifyPropertyChangedBase
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("released_at")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("info_json")]
        public InfoFile InfoFile { get; set; }


        // Removed from API

        //[JsonProperty("factorio_version")]
        //[JsonConverter(typeof(TwoPartVersionConverter))]
        //public Version FactorioVersion { get; set; }

        //[JsonProperty("downloads_count")]
        //public int DownloadCount { get; set; }


        bool isInstalled;
        bool isVersionInstalled;

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

        public bool IsVersionInstalled
        {
            get { return isVersionInstalled; }
            set
            {
                if (value != isVersionInstalled)
                {
                    isVersionInstalled = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsVersionInstalled)));
                }
            }
        }
    }
}
