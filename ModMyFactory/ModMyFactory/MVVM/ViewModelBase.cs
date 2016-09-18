using System.Windows;

namespace ModMyFactory.MVVM
{
    /// <summary>
    /// Base class of all view models.
    /// </summary>
    /// <typeparam name="T">The windows type the view model belongs to.</typeparam>
    abstract class ViewModelBase<T> : NotifyPropertyChangedBase, IViewModelBase<T> where T : Window, IViewModelBoundWindow<IViewModelBase<T>>
    {
        /// <summary>
        /// The window this view model is associated with.
        /// </summary>
        public T Window { get; set; }

        /// <summary>
        /// Creates a view model.
        /// </summary>
        protected ViewModelBase()
        { }
    }
}
