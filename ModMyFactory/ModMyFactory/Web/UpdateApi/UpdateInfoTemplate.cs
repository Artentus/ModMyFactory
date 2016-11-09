using Newtonsoft.Json;

namespace ModMyFactory.Web.UpdateApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class UpdateInfoTemplate
    {
        [JsonProperty("core-win32")]
        public UpdateStepTemplate[] Win32Package { get; set; }

        [JsonProperty("core-win64")]
        public UpdateStepTemplate[] Win64Package { get; set; }
    }
}
