using System.ComponentModel;

namespace ModMyFactory.Views
{
    partial class OnlineModsWindow
    {
        const int DefaultWidth = 800, DefaultHeight = 600;

        public OnlineModsWindow()
            : base(App.Instance.Settings.OnlineModsWindowInfo, DefaultWidth, DefaultHeight)
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
