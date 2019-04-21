using System;
using System.ComponentModel;
using System.Text;
using WPFCore;
using WPFCore.Commands;

namespace ModMyFactory.ViewModels
{
    sealed class AboutViewModel : ViewModelBase
    {
        public string VersionString
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append('v');
                sb.Append(App.Version);

                #if PORTABLE
                sb.Append(" portable");
                #endif

                return sb.ToString();
            }
        }

        public bool PageState { get; private set; }

        public Uri PageUri { get; private set; }

        public RelayCommand SwitchPageCommand { get; }

        public AboutViewModel()
        {
            PageUri = new Uri("AboutPage.xaml", UriKind.Relative);
            SwitchPageCommand = new RelayCommand(SwitchPage);
        }

        private void SwitchPage()
        {
            if (PageState)
            {
                PageUri = new Uri("AboutPage.xaml", UriKind.Relative);
            }
            else
            {
                PageUri = new Uri("ChangelogPage.xaml", UriKind.Relative);
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(PageUri)));

            PageState = !PageState;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(PageState)));
        }
    }
}
