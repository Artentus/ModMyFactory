using WPFCore.Windows;

namespace ModMyFactory.Controls
{
    abstract class LayoutRoundingWindow : ViewModelBoundWindow
    {
        protected LayoutRoundingWindow()
        {
            UseLayoutRounding = true;
        }
    }
}
