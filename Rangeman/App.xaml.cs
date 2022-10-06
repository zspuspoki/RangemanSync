using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App : Application
    {
        public App(Android.Content.Context context)
        {
            InitializeComponent();

            var tabbedPage = new Rangeman.MyTabbedPage(); //new MainPage(context);
            tabbedPage.BindingContext = new MyTabbedPageViewModel(context);

            MainPage = tabbedPage;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

    }
}