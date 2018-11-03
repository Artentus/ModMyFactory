using System;
using System.Collections.Generic;

namespace ModMyFactory.Models.ModSettings
{
    abstract class ListModSettingProxy<T> : ModSettingProxy<T>, IListModSetting<T> where T : IEquatable<T>
    {
        public override T Value
        {
            get => base.Value;
            set
            {
                var comparer = EqualityComparer<T>.Default;
                foreach (var allowedValue in AllowedValues)
                {
                    if (comparer.Equals(value, allowedValue))
                    {
                        base.Value = value;
                        return;
                    }
                }

                throw new ArgumentException("Value not allowed.", nameof(value));
            }
        }

        public IReadOnlyCollection<T> AllowedValues { get; }

        protected ListModSettingProxy(ListModSetting<T> baseSetting)
            : base(baseSetting)
        {
            AllowedValues = baseSetting.AllowedValues;
        }

        protected ListModSettingProxy(ListModSettingProxy<T> baseSetting)
            : base(baseSetting)
        {
            AllowedValues = baseSetting.AllowedValues;
        }
    }
}
