using System.ComponentModel;

namespace ModMyFactory
{
    sealed class DownloadViewModel : NotifyPropertyChangedBase
    {
        double progress;
        string downloadState;
        bool isIndeterminate;

        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public string DownloadState
        {
            get { return downloadState; }
            set
            {
                if (value != downloadState)
                {
                    downloadState = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DownloadState)));
                }
            }
        }

        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                if (value != isIndeterminate)
                {
                    isIndeterminate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsIndeterminate)));
                }
            }
        }

        public RelayCommand CancelCommand { get; }

        public DownloadViewModel(RelayCommand cancelCommand)
        {
            CancelCommand = cancelCommand;
        }
    }
}
