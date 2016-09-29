using System.Diagnostics;
using ModMyFactory.MVVM;

namespace ModMyFactory
{
    sealed class AboutViewModel : ViewModelBase<AboutWindow>
    {
        public string VersionString => "v" + App.Instance.AssemblyVersion.ToString(3);

        public RelayCommand Url1Command { get; }

        public RelayCommand Url2Command { get; }

        public RelayCommand Url3Command { get; }

        public RelayCommand Url4Command { get; }


        public RelayCommand Contributor1Command { get; }

        public RelayCommand Contributor2Command { get; }

        public AboutViewModel()
        {
            Url1Command = new RelayCommand(() => Process.Start("http://www.iconarchive.com/show/flag-icons-by-famfamfam.html"));
            Url2Command = new RelayCommand(() => Process.Start("http://www.dafont.com/sylar-stencil.font"));
            Url3Command = new RelayCommand(() => Process.Start("http://www.ookii.org/software/dialogs/"));
            Url4Command = new RelayCommand(() => Process.Start("http://www.newtonsoft.com/json"));

            Contributor1Command = new RelayCommand(() => Process.Start("https://github.com/plague006"));
            Contributor2Command = new RelayCommand(() => Process.Start("https://github.com/jodli"));
        }
    }
}
