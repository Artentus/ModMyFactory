using ModMyFactory.Helpers;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ModMyFactory.Web.ModApi
{
    [JsonObject(MemberSerialization.OptIn)]
    sealed class ExtendedModInfo : ModInfo
    {
        private static Uri BaseDataUri { get; }

        static ExtendedModInfo()
        {
            const string dataUrl = "https://mods-data.factorio.com";
            BaseDataUri = new Uri(dataUrl, UriKind.Absolute);
        }


        [JsonProperty("releases")]
        public ModRelease[] Releases { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        [JsonProperty("github_path")]
        public string GitHubUrl { get; set; }

        [JsonProperty("license")]
        public LicenseInfo License { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("faq")]
        public string Faq { get; set; }
        
        [JsonProperty("thumbnail")]
        public string ThumbnailUrl { get; set; }

        public override ModRelease LatestRelease { get => GetLatestRelease(); set => base.LatestRelease = value; }

        public ModRelease GetLatestRelease(Version factorioVersion = null)
        {
            if (factorioVersion == null)
                return Releases.MaxBy(release => release.Version, new VersionComparer());
            else
                return Releases.Where(release => release.InfoFile.FactorioVersion == factorioVersion).MaxBy(release => release.Version, new VersionComparer());
        }

        public Uri GetFullThumbnailUri()
        {
            if (string.IsNullOrEmpty(ThumbnailUrl)) return null;
            return new Uri(BaseDataUri, ThumbnailUrl);
        }
    }
}
