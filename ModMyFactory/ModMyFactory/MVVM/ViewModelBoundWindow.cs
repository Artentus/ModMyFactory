using System;
using System.Windows;

namespace ModMyFactory.MVVM
{
    /// <summary>
    /// Base class of all windows that have a view model bound as data context.
    /// </summary>
    /// <typeparam name="T">THe view models type the window belongs to.</typeparam>
    abstract class ViewModelBoundWindow<T> : Window where T : IViewModelBase
    {
        /// <summary>
        /// The view model bound to this window.
        /// </summary>
        public T ViewModel => (T)DataContext;

        protected ViewModelBoundWindow()
        {
            UseLayoutRounding = true;
        } 

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ViewModel.SetWindow(this);
        }
    }
}
