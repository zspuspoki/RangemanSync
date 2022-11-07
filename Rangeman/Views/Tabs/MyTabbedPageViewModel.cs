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
            ConfigPageViewModel.UseMbTilesClicked += ConfigPageViewModel_UseMbTilesClicked;
        }

        private void ConfigPageViewModel_UseMbTilesClicked(object sender, System.EventArgs e)
        {
            //MbTiles is enabled on the config page, so MapPageViewModel should be notified about the changes
            MapPageViewModel.UpdateMapToUseMbTilesFile();
        }
    }
}