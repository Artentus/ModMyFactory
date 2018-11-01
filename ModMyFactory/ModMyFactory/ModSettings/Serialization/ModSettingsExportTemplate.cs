using ModMyFactory.Models.ModSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ModMyFactory.ModSettings.Serialization
{
    class ModSettingsExportTemplate
    {
        [JsonProperty("startup")]
        public Dictionary<string, ModSettingValueTemplate> StartupTemplates { get; }

        [JsonProperty("runtime-global")]
        public Dictionary<string, ModSettingValueTemplate> RuntimeGlobalTemplates { get; }

        [JsonProperty("runtime-per-user")]
        public Dictionary<string, ModSettingValueTemplate> RuntimeUserTemplates { get; }

        public ModSettingsExportTemplate()
        {
            StartupTemplates = new Dictionary<string, ModSettingValueTemplate>();
            RuntimeGlobalTemplates = new Dictionary<string, ModSettingValueTemplate>();
            RuntimeUserTemplates = new Dictionary<string, ModSettingValueTemplate>();
        }

        [JsonConstructor]
        public ModSettingsExportTemplate(Dictionary<string, ModSettingValueTemplate> startupTemplates, Dictionary<string, ModSettingValueTemplate> runtimeGlobalTemplates, Dictionary<string, ModSettingValueTemplate> runtimeUserTemplates)
        {
            StartupTemplates = startupTemplates;
            RuntimeGlobalTemplates = runtimeGlobalTemplates;
            RuntimeUserTemplates = runtimeUserTemplates;
        }

        public void AddSetting<T>(IModSetting<T> setting) where T : IEquatable<T>
        {
            var template = new ModSettingValueTemplate(setting.Value);

            switch (setting.LoadTime)
            {
                case LoadTime.Startup:
                    StartupTemplates[setting.Name] = template;
                    break;
                case LoadTime.RuntimeGlobal:
                    RuntimeGlobalTemplates[setting.Name] = template;
                    break;
                case LoadTime.RuntimeUser:
                    RuntimeUserTemplates[setting.Name] = template;
                    break;
            }
        }

        public bool TryGetValue<T>(IModSetting<T> setting, out T value) where T : IEquatable<T>
        {
            Dictionary<string, ModSettingValueTemplate> dict;

            switch (setting.LoadTime)
            {
                case LoadTime.Startup:
                    dict = StartupTemplates;
                    break;
                case LoadTime.RuntimeGlobal:
                    dict = RuntimeGlobalTemplates;
                    break;
                case LoadTime.RuntimeUser:
                    dict = RuntimeUserTemplates;
                    break;
                default:
                    value = default(T);
                    return false;
            }
            
            bool hasValue = dict.TryGetValue(setting.Name, out var template);
            if (hasValue && (template != null))
            {
                value = template.GetValue<T>();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }
}
