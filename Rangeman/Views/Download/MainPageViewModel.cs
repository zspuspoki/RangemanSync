﻿using Android.Content;
using System.Collections.ObjectModel;

namespace Rangeman
{
    internal class MainPageViewModel : ViewModelBase
    {
        private string progressMessage;

        public MainPageViewModel(Android.Content.Context context)
        {
            Context = context;
        }

        public Context Context { get; }
        public ObservableCollection<LogHeaderViewModel> LogHeaderList { get; } = new ObservableCollection<LogHeaderViewModel>();
        public LogHeaderViewModel SelectedLogHeader { get; set; }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
    }
}