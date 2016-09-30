using System;
using System.ComponentModel;
using System.Windows.Shell;
using ModMyFactory.MVVM;
using ModMyFactory.Views;

namespace ModMyFactory.ViewModels
{
    sealed class ProgressViewModel : ViewModelBase<ProgressWindow>
    {
        double progress;
        string actionName;
        string progressDescription;
        bool isIndeterminate;

        public event EventHandler CancelRequested;

        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                if (Window.TaskbarItemInfo != null) Window.TaskbarItemInfo.ProgressValue = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public string ActionName
        {
            get { return actionName; }
            set
            {
                if (value != actionName)
                {
                    actionName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ActionName)));
                }
            }
        }

        public string ProgressDescription
        {
            get { return progressDescription; }
            set
            {
                if (value != progressDescription)
                {
                    progressDescription = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ProgressDescription)));
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
                    if (Window.TaskbarItemInfo != null)
                    {
                        Window.TaskbarItemInfo.ProgressState = value
                            ? TaskbarItemProgressState.Indeterminate
                            : TaskbarItemProgressState.Normal;
                    }
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsIndeterminate)));
                }
            }
        }

        public bool CanCancel { get; set; }

        public RelayCommand CancelCommand { get; }

        public ProgressViewModel()
        {
            CancelCommand = new RelayCommand(() => CancelRequested?.Invoke(this, EventArgs.Empty), () => CanCancel);
        }
    }
}
