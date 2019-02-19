using System.IO;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    sealed class ChangelogPageViewModel : ViewModelBase
    {
        static ChangelogPageViewModel instance;

        public static ChangelogPageViewModel Instance => instance ?? (instance = new ChangelogPageViewModel());

        public string Changelog { get; }

        private ChangelogPageViewModel()
        {
            if (!App.IsInDesignMode)
            {
                string changelogPath = Path.Combine(App.Instance.ApplicationDirectoryPath, "Changelog.txt");
                var changelogFile = new FileInfo(changelogPath);

                if (changelogFile.Exists)
                {
                    using (var reader = changelogFile.OpenText())
                        Changelog = reader.ReadToEnd();
                }
                else
                {
                    Changelog = string.Empty;
                }
            }
        }
    }
}
