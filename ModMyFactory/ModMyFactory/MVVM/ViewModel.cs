using System.Windows;

namespace ModMyFactory.MVVM
{
    static class ViewModel
    {
        /// <summary>
        /// Creates a view model of the specified type from the given window.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="window">The window the view model is created from.</param>
        /// <returns>Returns a view model  of the specified type created from the given window</returns>
        public static TViewModel CreateFromWindow<TViewModel>(Window window) where TViewModel : ViewModelBase
        {
            return (TViewModel)ViewModelBase.GetFromWindow(window);
        }
    }
}
