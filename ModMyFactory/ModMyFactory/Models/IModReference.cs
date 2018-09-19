using System.ComponentModel;
using System.Windows.Media.Imaging;
using WPFCore.Commands;

namespace ModMyFactory.Models
{
    interface IModReference : INotifyPropertyChanged
    {
        string DisplayName { get; }

        string VersionInfo { get; }

        BitmapImage Image { get; }

        bool? Active { get; set; }

        bool HasUnsatisfiedDependencies { get; }

        RelayCommand RemoveFromParentCommand { get; }
    }
}
