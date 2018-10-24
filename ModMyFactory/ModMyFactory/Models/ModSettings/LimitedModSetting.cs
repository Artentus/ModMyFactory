using ModMyFactory.ModSettings;
using System;
using System.Collections.Generic;

namespace ModMyFactory.Models.ModSettings
{
    abstract class LimitedModSetting<T> : ModSetting<T>, ILimitedModSetting<T> where T : IEquatable<T>, IComparable<T>
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

        protected LimitedModSetting(string name, LoadTime loadTime, string ordering, T defaultValue, T minValue, T maxValue)
            : base(name, loadTime, ordering, defaultValue)
        {
            var comparer = Comparer<T>.Default;
            if (comparer.Compare(minValue, maxValue) > 0) throw new ArgumentException("Min value cannot be larger than max value.");
            if (comparer.Compare(defaultValue, minValue) < 0) throw new ArgumentOutOfRangeException(nameof(defaultValue));
            if (comparer.Compare(defaultValue, maxValue) > 0) throw new ArgumentOutOfRangeException(nameof(defaultValue));

            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
