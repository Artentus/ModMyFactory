using System.Windows;

namespace ModMyFactory.MVVM
{
    interface IViewModelBase<out T> where T : Window
    {
        T Window { get; }
    }
}
