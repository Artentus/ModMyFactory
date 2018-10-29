using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ModMyFactory.Models.ModSettings
{
    abstract class ModSettingProxy<T> : ModSetting<T>, IModSettingProxy<T> where T : IEquatable<T>
    {
        readonly ModSetting<T> baseSetting;
        bool @override;
        T value;

        public bool Override
        {
            get => @override;
            set
            {
                if (value != @override)
                {
                    @override = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Override)));

                    if (@override) this.value = baseSetting.Value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public override T Value
        {
            get => Override ? value : baseSetting.Value;
            set
            {
                var comparer = EqualityComparer<T>.Default;
                if (!comparer.Equals(value, this.value))
                {
                    this.value = value;
                    if (!Override) OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public override DataTemplate Template => baseSetting.Template;

        protected ModSettingProxy(ModSetting<T> baseSetting)
            : base(baseSetting.Owner, baseSetting.Name, baseSetting.LoadTime, baseSetting.Ordering, baseSetting.DefaultValue)
        {
            this.baseSetting = baseSetting;
            value = baseSetting.Value;
        }
    }
}
