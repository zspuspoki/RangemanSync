namespace Rangeman
{
    internal class MyTabbedPageViewModel
    {
        public MainPageViewModel MainPageViewModel { get; set; }
        public MapPageViewModel MapPageViewModel { get; set; }
        public ConfigPageViewModel ConfigPageViewModel { get; set; }

        public MyTabbedPageViewModel(Android.Content.Context context)
        {
            MainPageViewModel = new MainPageViewModel(context);
            MapPageViewModel = new MapPageViewModel(context);
            ConfigPageViewModel = new ConfigPageViewModel();
        }
    }
}