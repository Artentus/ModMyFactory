using System.Diagnostics;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed class AboutViewModel : ViewModelBase
    {
        public string VersionString => $"v{App.Version}";

        public RelayCommand Url1Command { get; }

        public RelayCommand Url2Command { get; }

        public RelayCommand Url3Command { get; }

        public RelayCommand Url4Command { get; }

        public RelayCommand Url5Command { get; }

        public RelayCommand Url6Command { get; }

        public RelayCommand Url7Command { get; }

        public RelayCommand Url8Command { get; }


        public RelayCommand Contributor1Command { get; }

        public RelayCommand Contributor2Command { get; }

        public RelayCommand Contributor3Command { get; }

        public RelayCommand Contributor4Command { get; }

        public RelayCommand Contributor5Command { get; }

        public RelayCommand Contributor6Command { get; }

        public RelayCommand Contributor7Command { get; }

        public RelayCommand Translator1Command { get; }

        public RelayCommand Translator2Command { get; }

        public RelayCommand Translator3Command { get; }

        public RelayCommand Translator4Command { get; }

        public RelayCommand Translator5Command { get; }

        public AboutViewModel()
        {
            Url1Command = new RelayCommand(() => Process.Start("http://www.iconarchive.com/show/flag-icons-by-famfamfam.html"));
            Url2Command = new RelayCommand(() => Process.Start("http://www.dafont.com/sylar-stencil.font"));
            Url3Command = new RelayCommand(() => Process.Start("http://www.ookii.org/software/dialogs/"));
            Url4Command = new RelayCommand(() => Process.Start("http://www.newtonsoft.com/json"));
            Url5Command = new RelayCommand(() => Process.Start("https://github.com/octokit/octokit.net"));
            Url6Command = new RelayCommand(() => Process.Start("http://www.zlib.net/"));
            Url7Command = new RelayCommand(() => Process.Start("https://github.com/pleonex/xdelta-sharp"));
            Url8Command = new RelayCommand(() => Process.Start("https://github.com/rickyah/ini-parser"));

            Contributor1Command = new RelayCommand(() => Process.Start("https://github.com/plague006"));
            Contributor2Command = new RelayCommand(() => Process.Start("https://github.com/jodli"));
            Contributor3Command = new RelayCommand(() => Process.Start("https://github.com/mpwoz"));
            Contributor4Command = new RelayCommand(() => Process.Start("https://github.com/credomane"));
            Contributor5Command = new RelayCommand(() => Process.Start("https://github.com/distantcam"));
            Contributor6Command = new RelayCommand(() => Process.Start("https://github.com/h4n9u1"));
            Contributor7Command = new RelayCommand(() => Process.Start("https://github.com/Polar-Zero"));

            Translator1Command = new RelayCommand(() => Process.Start("https://github.com/Averssem"));
            Translator2Command = new RelayCommand(() => Process.Start("https://www.reddit.com/user/blackbat24"));
            Translator3Command = new RelayCommand(() => Process.Start("https://github.com/Corwin616"));
            Translator4Command = new RelayCommand(() => Process.Start("https://github.com/JAMESY9868"));
            Translator5Command = new RelayCommand(() => Process.Start("https://github.com/GimoXagros"));
        }
    }
}
