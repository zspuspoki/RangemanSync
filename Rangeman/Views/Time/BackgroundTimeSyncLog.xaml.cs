using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman.Views.Time
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BackgroundTimeSyncLog : ContentPage
    {
        public BackgroundTimeSyncLog()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(ViewModel != null)
            {
                ViewModel.ReloadLogMessages();
            }
        }

        public BackgroundTimeSyncLogViewModel ViewModel
        {
            get
            {
                var vm = BindingContext as BackgroundTimeSyncLogViewModel;
                return vm;
            }
        }
    }
}