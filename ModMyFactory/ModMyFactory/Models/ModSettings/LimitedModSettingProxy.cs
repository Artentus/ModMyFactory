using System;
using System.Collections.Generic;

namespace ModMyFactory.Models.ModSettings
{
    abstract class LimitedModSettingProxy<T> : ModSettingProxy<T>, ILimitedModSetting<T> where T : IEquatable<T>, IComparable<T>
    {
        public override T Value
        {
            get => base.Value;
            set
            {
                T newValue = value;

                var comparer = Comparer<T>.Default;
                if (comparer.Compare(newValue, MinValue) < 0) newValue = MinValue;
                if (comparer.Compare(newValue, MaxValue) > 0) newValue = MaxValue;

                base.Value = newValue;
            }
        }

        public T MinValue { get; }

        public T MaxValue { get; }

        protected LimitedModSettingProxy(LimitedModSetting<T> baseSetting)
            : base(baseSetting)
        {
            MinValue = baseSetting.MinValue;
            MaxValue = baseSetting.MaxValue;
        }

        protected LimitedModSettingProxy(LimitedModSettingProxy<T> baseSetting)
            : base(baseSetting)
        {
            MinValue = baseSetting.MinValue;
            MaxValue = baseSetting.MaxValue;
        }
    }
}
