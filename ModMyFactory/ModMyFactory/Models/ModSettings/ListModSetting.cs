using ModMyFactory.ModSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModMyFactory.Models.ModSettings
{
    abstract class ListModSetting<T> : ModSetting<T>, IListModSetting<T> where T : IEquatable<T>
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

        protected ListModSetting(IHasModSettings owner, string name, LoadTime loadTime, string ordering, T defaultValue, IEnumerable<T> allowedValues)
            : base(owner, name, loadTime, ordering, defaultValue)
        {
            var comparer = EqualityComparer<T>.Default;
            foreach (var allowedValue in allowedValues)
            {
                if (comparer.Equals(defaultValue, allowedValue))
                {
                    AllowedValues = new ReadOnlyCollection<T>(new List<T>(allowedValues));
                    return;
                }
            }

            throw new ArgumentException("Value not allowed.", nameof(defaultValue));
        }
    }
}
