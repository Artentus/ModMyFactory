using System.Diagnostics;
using System.Reflection;

namespace ModMyFactory
{
    sealed class AboutViewModel : NotifyPropertyChangedBase
    {
        static AboutViewModel instance;

        public static AboutViewModel Instance => instance ?? (instance = new AboutViewModel());

        public string VersionString => "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public RelayCommand Url1Command { get; }

        public RelayCommand Url2Command { get; }

        public RelayCommand Url3Command { get; }

        public RelayCommand Url4Command { get; }

        private AboutViewModel()
        {
            Url1Command = new RelayCommand(() => Process.Start("http://www.iconarchive.com/show/flag-icons-by-famfamfam.html"));
            Url2Command = new RelayCommand(() => Process.Start("http://www.dafont.com/sylar-stencil.font"));
            Url3Command = new RelayCommand(() => Process.Start("http://www.ookii.org/software/dialogs/"));
            Url4Command = new RelayCommand(() => Process.Start("http://www.newtonsoft.com/json"));
        }
    }
}
