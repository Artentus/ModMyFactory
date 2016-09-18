using System.Windows;

namespace ModMyFactory.MVVM
{
    interface IViewModelBoundWindow<out T> where T : IViewModelBase<Window>
    {
        T ViewModel { get; }
    }
}
