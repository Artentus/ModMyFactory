using System.Windows.Input;

namespace ModMyFactory
{
    static class Commands
    {
        static ICommand maximize;
        public static ICommand Maximize => maximize ?? (maximize = new RoutedCommand());

        static ICommand minimize;
        public static ICommand Minimize => minimize ?? (minimize = new RoutedCommand());
    }
}
