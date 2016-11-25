using WPFCore.Windows;

namespace ModMyFactory.MVVM
{
    abstract class LayoutRoundingWindow : ViewModelBoundWindow
    {
        protected LayoutRoundingWindow()
        {
            UseLayoutRounding = true;
        }
    }
}
