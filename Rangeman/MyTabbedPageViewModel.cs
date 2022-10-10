using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rangeman
{
    internal class MyTabbedPageViewModel
    {
        public MainPageViewModel MainPageViewModel { get; set; }
        public MapPageViewModel MapPageViewModel { get; set; }

        public MyTabbedPageViewModel(Android.Content.Context context)
        {
            MainPageViewModel = new MainPageViewModel(context);
            MapPageViewModel = new MapPageViewModel(context);
        }
    }
}