using Android.Content;
using System.Collections.ObjectModel;

namespace Rangeman
{
    public class MainPageViewModel : ViewModelBase
    {
        private string progressMessage;
        private bool watchCommandButtonsAreVisible = true;
        private bool disconnectButtonIsVisible = false;

        public MainPageViewModel(Context context)
        {
            Context = context;
        }

        public Context Context { get; }
        public ObservableCollection<LogHeaderViewModel> LogHeaderList { get; } = new ObservableCollection<LogHeaderViewModel>();
        public LogHeaderViewModel SelectedLogHeader { get; set; }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        
        public bool WatchCommandButtonsAreVisible { get => watchCommandButtonsAreVisible; set { watchCommandButtonsAreVisible = value; OnPropertyChanged("WatchCommandButtonsAreVisible"); } }
        public bool DisconnectButtonIsVisible 
        { 
            get => disconnectButtonIsVisible; 
            set 
            { 
                disconnectButtonIsVisible = value; 
                OnPropertyChanged("DisconnectButtonIsVisible");
                WatchCommandButtonsAreVisible = !value;
            } 
        }
    }
}