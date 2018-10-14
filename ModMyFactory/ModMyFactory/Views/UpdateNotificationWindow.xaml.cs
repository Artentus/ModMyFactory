using ModMyFactory.Web;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ModMyFactory.Views
{
    partial class UpdateNotificationWindow
    {
        public UpdateNotificationWindow()
        {
            InitializeComponent();

            this.Loaded += WindowLoadedHandler;
        }

        private void UpdateButtonClickHandler(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private async void WindowLoadedHandler(object sender, RoutedEventArgs e)
        {
            const string changelogUrl = "https://raw.githubusercontent.com/Artentus/ModMyFactory/master/CHANGELOG.md";

            try
            {
                string document = await Task.Run(() => WebHelper.GetDocument(changelogUrl, null));
                ChangelogTextBlock.Text = document;
            }
            catch (WebException)
            { }
        }
    }
}
