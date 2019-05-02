using ModMyFactory.ModSettings;
using ModMyFactory.ModSettings.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models.ModSettings
{
    abstract class ModSetting<T> : NotifyPropertyChangedBase, IModSetting<T> where T : IEquatable<T>
    {
        T value;

        public IHasModSettings Owner { get; }

        public string Name { get; }

        public LoadTime LoadTime { get; }

        public string Ordering { get; }

        public ICommand ResetCommand { get; }

        public virtual T Value
        {
            get => value;
            set
            {
                var comparer = EqualityComparer<T>.Default;
                if (!comparer.Equals(value, this.value))
                {
                    this.value = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public T DefaultValue { get; }

        public abstract DataTemplate Template { get; }

        private T GetStartValue()
        {
            if (ModSettingsManager.TryGetSavedValue(Owner, this, out T value)) return value;
            return DefaultValue;
        }

        protected ModSetting(IHasModSettings owner, string name, LoadTime loadTime, string ordering, T defaultValue)
        {
            Owner = owner;
            Name = name;
            LoadTime = loadTime;
            Ordering = ordering;
            ResetCommand = new RelayCommand(Reset);

            DefaultValue = defaultValue;
            value = GetStartValue();
        }
        
        public virtual void Reset()
        {
            Value = DefaultValue;
        }

        public abstract IModSettingProxy CreateProxy();

        public ModSettingValueTemplate CreateValueTemplate() => new ModSettingValueTemplate(value);
    }
}
