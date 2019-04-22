using ModMyFactory.Web;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ModMyFactory.Views
{
    partial class UpdateNotificationWindow
    {
        public ExtendedVersion Version { get; }

        public bool CanAuto { get; }

        public bool Auto { get; private set; }

        public UpdateNotificationWindow()
        {
            InitializeComponent();

            this.Loaded += WindowLoadedHandler;
            Version = new ExtendedVersion(1, 0);
            DataContext = this;
            CanAuto = false;
        }

        public UpdateNotificationWindow(ExtendedVersion version, bool canAuto)
        {
            InitializeComponent();

            this.Loaded += WindowLoadedHandler;
            Version = version;
            DataContext = this;
            CanAuto = canAuto;
        }

        private void ManualButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Auto = false;
        }

        private void AutoButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Auto = true;
        }

        private static async Task<string> DownloadChangelogAsync(ExtendedVersion version)
        {
            const string changelogUrl = "https://raw.githubusercontent.com/Artentus/ModMyFactory/master/CHANGELOG.md";
            string document = string.Empty;

            try
            {
                document = await Task.Run(() => WebHelper.GetDocument(changelogUrl));

                int i = document.IndexOf($"#### {version}");
                if (i >= 0) document = document.Substring(i);
            }
            catch (WebException)
            { }

            return document;
        }

        private async void WindowLoadedHandler(object sender, RoutedEventArgs e)
        {
            ChangelogTextBlock.Text = await DownloadChangelogAsync(Version);
        }
    }
}
