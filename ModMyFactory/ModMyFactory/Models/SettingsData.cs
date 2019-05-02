using ModMyFactory.ModSettings;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using ModMyFactory.Models.ModSettings;
using System.Linq;

namespace ModMyFactory.Models
{
    partial class ModFile
    {
        private class SettingsData
        {
            static readonly Dictionary<string, SettingType> settingTypeMapping;
            static readonly Dictionary<string, LoadTime> loadTimeMapping;

            static SettingsData()
            {
                settingTypeMapping = new Dictionary<string, SettingType>
                {
                    { "bool-setting", SettingType.Boolean },
                    { "int-setting", SettingType.Integer },
                    { "double-setting", SettingType.FloatingPoint },
                    { "string-setting", SettingType.String },
                };
                loadTimeMapping = new Dictionary<string, LoadTime>
                {
                    { "startup", LoadTime.Startup },
                    { "runtime-global", LoadTime.RuntimeGlobal },
                    { "runtime-per-user", LoadTime.RuntimeUser },
                };
            }


            readonly List<Dictionary<string, DynValue>> data;
            
            public SettingsData()
            {
                data = new List<Dictionary<string, DynValue>>();
            }

            public void Extend(List<Dictionary<string, DynValue>> value)
            {
                data.AddRange(value);
            }

            private bool TryConvertSettingType(string value, out SettingType result)
            {
                return settingTypeMapping.TryGetValue(value, out result);
            }

            private bool TryConvertNumberTable(IEnumerable<DynValue> table, out IEnumerable<decimal> result)
            {
                var list = new List<decimal>();
                result = list;

                foreach (var dyn in table)
                {
                    if (dyn.Type == DataType.Number) list.Add((decimal)dyn.Number);
                    else return false;
                }

                return true;
            }

            private bool TryConvertStringTable(IEnumerable<DynValue> table, out IEnumerable<string> result)
            {
                var list = new List<string>();
                result = list;

                foreach (var dyn in table)
                {
                    if (dyn.Type == DataType.String) list.Add(dyn.String);
                    else return false;
                }

                return true;
            }

            private bool TryConvert(Dictionary<string, DynValue> dict, IHasModSettings owner, out IModSetting result)
            {
                result = null;

                if (!dict.TryGetValue("type", out var typeDyn)) return false;
                if ((typeDyn.Type != DataType.String) || !settingTypeMapping.TryGetValue(typeDyn.String, out var type)) return false;

                if (!dict.TryGetValue("name", out var nameDyn)) return false;
                if (nameDyn.Type != DataType.String) return false;
                string name = nameDyn.String;

                if (!dict.TryGetValue("setting_type", out var loadTimeDyn)) return false;
                if ((loadTimeDyn.Type != DataType.String) || !loadTimeMapping.TryGetValue(loadTimeDyn.String, out var loadTime)) return false;

                string ordering = string.Empty;
                if (dict.TryGetValue("order", out var orderingDyn) && (orderingDyn.Type == DataType.String))
                    ordering = orderingDyn.String;

                bool hasDefaultValue = dict.TryGetValue("default_value", out var defaultValueDyn);

                bool isList = dict.TryGetValue("allowed_values", out var allowedValuesDyn);
                if (isList && (allowedValuesDyn.Type != DataType.Table)) return false;
                
                if (isList)
                {
                    var allowedValuesListDyn = allowedValuesDyn.Table.Values;

                    switch (type)
                    {
                        case SettingType.Boolean:
                            return false;
                        case SettingType.Integer:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.Number)) return false;
                            if (!TryConvertNumberTable(allowedValuesListDyn, out var allowedIntValues)) return false;
                            result = new IntegerListModSetting(owner, name, loadTime, ordering, hasDefaultValue ? (long)defaultValueDyn.Number : 0, allowedIntValues.Select(value => (long)value));
                            break;
                        case SettingType.FloatingPoint:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.Number)) return false;
                            if (!TryConvertNumberTable(allowedValuesListDyn, out var allowedFloatValues)) return false;
                            result = new FloatingPointListModSetting(owner, name, loadTime, ordering, hasDefaultValue ? (decimal)defaultValueDyn.Number : 0, allowedFloatValues);
                            break;
                        case SettingType.String:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.String)) return false;
                            if (!TryConvertStringTable(allowedValuesListDyn, out var allowedStringValues)) return false;
                            result = new StringListModSetting(owner, name, loadTime, ordering, hasDefaultValue ? defaultValueDyn.String : string.Empty, allowedStringValues);
                            break;
                    }
                }
                else
                {
                    bool hasMinValue = dict.TryGetValue("minimum_value", out var minValueDyn);
                    if (hasMinValue && (minValueDyn.Type != DataType.Number)) return false;

                    bool hasMaxValue = dict.TryGetValue("maximum_value", out var maxValueDyn);
                    if (hasMaxValue && (maxValueDyn.Type != DataType.Number)) return false;

                    switch (type)
                    {
                        case SettingType.Boolean:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.Boolean)) return false;
                            result = new BooleanModSetting(owner, name, loadTime, ordering, hasDefaultValue ? defaultValueDyn.Boolean : false);
                            break;
                        case SettingType.Integer:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.Number)) return false;
                            result = new IntegerModSetting(owner, name, loadTime, ordering, hasDefaultValue ? (long)defaultValueDyn.Number : 0, hasMinValue ? (long)minValueDyn.Number : long.MinValue, hasMaxValue ? (long)maxValueDyn.Number : long.MaxValue);
                            break;
                        case SettingType.FloatingPoint:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.Number)) return false;
                            result = new FloatingPointModSetting(owner, name, loadTime, ordering, hasDefaultValue ? (decimal)defaultValueDyn.Number : 0, hasMinValue ? (decimal)minValueDyn.Number : decimal.MinValue, hasMaxValue ? (decimal)maxValueDyn.Number : decimal.MaxValue);
                            break;
                        case SettingType.String:
                            if (hasDefaultValue && (defaultValueDyn.Type != DataType.String)) return false;
                            bool allowEmptyValue = false;
                            if (dict.TryGetValue("allow_blank", out var allowEmptyValueDyn) && (allowEmptyValueDyn.Type == DataType.Boolean))
                                allowEmptyValue = allowEmptyValueDyn.Boolean;
                            result = new StringModSetting(owner, name, loadTime, ordering, hasDefaultValue ? defaultValueDyn.String : string.Empty, allowEmptyValue);
                            break;
                    }
                }

                return true;
            }

            public IList<IModSetting> ToSettings(IHasModSettings owner)
            {
                var result = new List<IModSetting>(data.Count);
                foreach (var dict in data)
                {
                    if (TryConvert(dict, owner, out var settingInfo))
                        result.Add(settingInfo);
                }
                return result;
            }
        }
    }
}
