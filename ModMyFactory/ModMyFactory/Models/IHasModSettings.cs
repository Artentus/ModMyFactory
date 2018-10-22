using ModMyFactory.Models.ModSettings;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace ModMyFactory.Models
{
    interface IHasModSettings
    {
        string DisplayName { get; }

        bool Override { get; set; }

        IReadOnlyCollection<IModSetting> Settings { get; }
        
        ICollectionView SettingsView { get; }

        ICommand ViewSettingsCommand { get; }

        void ViewSettings();
    }
}
