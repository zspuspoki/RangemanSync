using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman.Views.Time
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NTPTime : ContentPage
    {
        public NTPTime()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (ViewModel != null)
            {
                ViewModel.RefreshServiceButtonStates();
            }
        }

        public NTPTimeViewModel ViewModel
        {
            get
            {
                var vm = BindingContext as NTPTimeViewModel;
                return vm;
            }
        }
    }
}