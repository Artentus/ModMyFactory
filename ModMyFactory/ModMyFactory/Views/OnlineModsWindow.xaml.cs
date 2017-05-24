using System.ComponentModel;

namespace ModMyFactory.Views
{
    partial class OnlineModsWindow
    {
        public OnlineModsWindow()
            : base(App.Instance.Settings.OnlineModsWindowInfo)
        {
            InitializeComponent();

            Closing += ClosingHandler;
        }

        private void ClosingHandler(object sender, CancelEventArgs e)
        {
            App.Instance.Settings.OnlineModsWindowInfo = CreateInfo();
            App.Instance.Settings.Save();
        }
    }
}
