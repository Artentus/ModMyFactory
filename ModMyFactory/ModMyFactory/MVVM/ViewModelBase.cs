using System.ComponentModel;
using System.Windows;

namespace ModMyFactory.MVVM
{
    /// <summary>
    /// Base class of all view models.
    /// </summary>
    /// <typeparam name="T">The windows type the view model belongs to.</typeparam>
    abstract class ViewModelBase<T> : NotifyPropertyChangedBase, IViewModelBase where T : Window
    {
        /// <summary>
        /// The window this view model is associated with.
        /// </summary>
        public T Window { get; private set; }

        void IViewModelBase.SetWindow(Window window)
        {
            if (window == null) Window = null;
            else Window = (T)window;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Window)));
        }
    }
}
