using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigPage : ContentPage
    {
        private bool checkBoxInitShouldBeDone = true;

        public ConfigPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (checkBoxInitShouldBeDone)
            {
                ViewModel.SetMbTilesCheckBoxesState();
                ViewModel.ProgressMessage = "";
                checkBoxInitShouldBeDone = false;
            }
        }

        public ConfigPageViewModel ViewModel
        {
            get
            {
                var vm = BindingContext as ConfigPageViewModel;
                return vm;
            }
        }
    }
}