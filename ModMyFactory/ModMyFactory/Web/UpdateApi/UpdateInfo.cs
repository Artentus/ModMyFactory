using System;
using Newtonsoft.Json;

namespace ModMyFactory.Web.UpdateApi
{
    [JsonObject(MemberSerialization.OptOut)]
    sealed class UpdateInfo
    {
        [JsonProperty("core-win32")]
        public UpdateStep[] Win32Package { get; }

        [JsonProperty("core-win64")]
        public UpdateStep[] Win64Package { get; }

        private static UpdateStep[] PreparePackage(UpdateStep[] package)
        {
            var result = new UpdateStep[package.Length - 1];

            Version stableVersion = null;

            for (int i = 0, j = 0; i < package.Length; i++)
            {
                UpdateStep step = package[i];

                if (step.IsStable)
                {
                    stableVersion = step.Version;
                }
                else
                {
                    result[j] = step;
                    j++;
                }
            }

            if (stableVersion != null)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    UpdateStep step = result[i];
                    if (step.Version == stableVersion)
                    {
                        result[i] = UpdateStep.StableFromStep(step);
                        break;
                    }
                }
            }

            return result;
        }

        [JsonConstructor]
        public UpdateInfo(UpdateStep[] win32Package, UpdateStep[] win64Package)
        {
            Win32Package = PreparePackage(win32Package);
            Win64Package = PreparePackage(win64Package);
        }
    }
}
