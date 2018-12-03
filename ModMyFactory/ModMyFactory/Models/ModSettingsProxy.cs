using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using ModMyFactory.Models.ModSettings;
using ModMyFactory.ModSettings;
using ModMyFactory.MVVM.Sorters;
using ModMyFactory.ViewModels;
using ModMyFactory.Views;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    sealed class ModSettingsProxy : NotifyPropertyChangedBase, IHasModSettings
    {
        readonly IHasModSettings baseMod;
        readonly Modpack parent;
        bool @override;
        IReadOnlyCollection<IModSettingProxy> settings;

        public string Name => baseMod.Name;

        public GameCompatibleVersion Version => baseMod.Version;

        public Version FactorioVersion => baseMod.FactorioVersion;

        public string DisplayName => baseMod.DisplayName;

        string IHasModSettings.UniqueID => $"modpack:{parent.Name}__{Name}_{Version}";

        bool IHasModSettings.UseBinaryFileOverride => false;

        public bool Override
        {
            get => @override;
            set
            {
                if (value != @override)
                {
                    @override = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Override)));

                    foreach (var setting in settings)
                        setting.Override = @override;
                }
            }
        }

        public IReadOnlyCollection<IModSetting> Settings => settings;

        public ICollectionView SettingsView { get; private set; }

        public bool HasSettings => baseMod.HasSettings;

        public ICommand ViewSettingsCommand { get; }

        public ModSettingsProxy(IHasModSettings baseMod, Modpack parent)
        {
            this.baseMod = baseMod;
            this.parent = parent;
            baseMod.PropertyChanged += PropertyChangedHandler;

            CreateView();
            ViewSettingsCommand = new RelayCommand(ViewSettings);

            Override = ModSettingsManager.HasSavedDataPresent(this);
        }

        ~ModSettingsProxy()
        {
            baseMod.PropertyChanged -= PropertyChangedHandler;
        }

        private void CreateView()
        {
            if (HasSettings)
            {
                var list = baseMod.Settings.Select(setting => setting.CreateProxy()).ToList();
                settings = new ReadOnlyCollection<IModSettingProxy>(list);
                var source = new CollectionViewSource() { Source = settings };
                var settingsView = (ListCollectionView)source.View;
                settingsView.CustomSort = new ModSettingSorter();
                settingsView.GroupDescriptions.Add(new PropertyGroupDescription("LoadTime"));
                SettingsView = settingsView;
            }
            else
            {
                settings = null;
                SettingsView = null;
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Settings)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SettingsView)));
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHasModSettings.Settings))
            {
                CreateView();
            }
            else if (e.PropertyName == nameof(IHasModSettings.HasSettings))
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasSettings)));
            }
        }

        public void ViewSettings()
        {
            var settingsWindow = new ModSettingsWindow() { Owner = App.Instance.MainWindow };
            var settingsViewModel = (ModSettingsViewModel)settingsWindow.ViewModel;
            settingsViewModel.SetMod(this);
            settingsWindow.ShowDialog();
        }

        public ILocale GetLocale(CultureInfo culture) => baseMod.GetLocale(culture);
    }
}
