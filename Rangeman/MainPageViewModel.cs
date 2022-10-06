using Android.Content;
using System.Collections.ObjectModel;

namespace Rangeman
{
    internal class MainPageViewModel
    {

        public MainPageViewModel(Android.Content.Context context)
        {
            Context = context;
        }

        public Context Context { get; }
        public ObservableCollection<ListItem> DeviceList { get; } = new ObservableCollection<ListItem>();
    }
}