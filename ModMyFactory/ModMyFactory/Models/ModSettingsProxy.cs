using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using ModMyFactory.Models.ModSettings;
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
        bool @override;
        IReadOnlyCollection<IModSettingProxy> settings;

        public string Name => baseMod.Name;

        public Version Version => baseMod.Version;

        public string DisplayName => baseMod.DisplayName;

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

        public ModSettingsProxy(IHasModSettings baseMod)
        {
            this.baseMod = baseMod;
            baseMod.PropertyChanged += PropertyChangedHandler;

            CreateView();
            ViewSettingsCommand = new RelayCommand(ViewSettings);
        }

        ~ModSettingsProxy()
        {
            baseMod.PropertyChanged -= PropertyChangedHandler;
        }

        private void CreateView()
        {
            var list = baseMod.Settings.Select(setting => setting.CreateProxy()).ToList();
            settings = new ReadOnlyCollection<IModSettingProxy>(list);
            var source = new CollectionViewSource() { Source = settings };
            var settingsView = (ListCollectionView)source.View;
            settingsView.CustomSort = new ModSettingSorter();
            settingsView.GroupDescriptions.Add(new PropertyGroupDescription("LoadTime"));
            SettingsView = settingsView;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Settings)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SettingsView)));
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHasModSettings.Settings))
            {
                CreateView();
            }
        }

        public void ViewSettings()
        {
            var settingsWindow = new ModSettingsWindow() { Owner = App.Instance.MainWindow };
            var settingsViewModel = (ModSettingsViewModel)settingsWindow.ViewModel;
            settingsViewModel.SetMod(this);
            settingsWindow.ShowDialog();
        }
    }
}
