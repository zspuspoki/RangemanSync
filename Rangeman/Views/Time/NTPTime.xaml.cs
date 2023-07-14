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