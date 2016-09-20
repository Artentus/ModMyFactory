using System.ComponentModel;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    interface IModReference : INotifyPropertyChanged
    {
        string DisplayName { get; }

        bool? Active { get; set; }

        RelayCommand RemoveFromParentCommand { get; }
    }
}
