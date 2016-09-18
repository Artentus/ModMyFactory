using System;
using System.Windows;

namespace ModMyFactory.MVVM
{
    /// <summary>
    /// Base class of all windows that have a view model bound as data context.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class ViewModelBoundWindow<T> : Window, IViewModelBoundWindow<T> where T : IViewModelBase<Window>
    {
        /// <summary>
        /// The view model bound to this window.
        /// </summary>
        public T ViewModel => (T)DataContext;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Type t = typeof(ViewModelBase<>);
            t = t.MakeGenericType(this.GetType());
            t.GetProperty("Window").SetValue(ViewModel, this);
        }
    }
}
