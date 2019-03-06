using System;
using System.ComponentModel;
using Newtonsoft.Json;
using WPFCore;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class ModRelease : NotifyPropertyChangedBase
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(GameVersionConverter))]
        public GameCompatibleVersion Version { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("released_at")]
        public DateTime ReleaseDate { get; set; }

        [JsonProperty("info_json")]
        public InfoFile InfoFile { get; set; }


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
