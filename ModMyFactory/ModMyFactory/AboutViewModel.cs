using System.Diagnostics;
using System.Reflection;

namespace ModMyFactory
{
    sealed class AboutViewModel : NotifyPropertyChangedBase
    {
        public string VersionString => "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public RelayCommand Url1Command { get; }

        public RelayCommand Url2Command { get; }

        public AboutViewModel()
        {
            Url1Command = new RelayCommand(() => Process.Start("http://www.iconarchive.com/show/flag-icons-by-famfamfam.html"));
            Url2Command = new RelayCommand(() => Process.Start("http://www.dafont.com/sylar-stencil.font"));
        }
    }
}
