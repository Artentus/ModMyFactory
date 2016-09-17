using System.Windows;

namespace ModMyFactory.MVVM
{
    abstract class ViewModelBase : NotifyPropertyChangedBase
    {
        protected abstract void SetWindow<TWindow>(TWindow window) where TWindow : Window;

        public static ViewModelBase GetFromWindow(Window window)
        {
            var viewModel = (ViewModelBase)window.DataContext;
            viewModel.SetWindow(window);
            return viewModel;
        }
    }

    /// <summary>
    /// Base class of all view models.
    /// </summary>
    /// <typeparam name="T">The windows type the view model belongs to.</typeparam>
    internal abstract class ViewModelBase<T> : ViewModelBase where T : Window
    {
        /// <summary>
        /// The window this view model is associated with.
        /// </summary>
        public T Window { get; private set; }

        /// <summary>
        /// Creates a view model.
        /// </summary>
        protected ViewModelBase()
        { }

        /// <summary>
        /// Creates a view model.
        /// </summary>
        /// <param name="window">The window this view model is associated with.</param>
        protected ViewModelBase(T window)
        {
            Window = window;
        }

        protected sealed override void SetWindow<TWindow>(TWindow window)
        {
            Window = (T)(Window)window;
        }
    }
}
