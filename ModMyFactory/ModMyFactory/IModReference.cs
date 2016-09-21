using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    interface IModReference : INotifyPropertyChanged
    {
        string DisplayName { get; }

        BitmapImage Image { get; }

        bool? Active { get; set; }

        List<IEditableCollectionView> ParentViews { get; }

        RelayCommand RemoveFromParentCommand { get; }
    }
}
