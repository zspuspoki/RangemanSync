using Android.App;
using Android.Webkit;
using Mapsui;
using Mapsui.Projection;
using Mapsui.Utilities;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {            
            InitializeComponent();

            InitalizeMap();
        }

        private async void InitalizeMap()
        {
            var map = new Mapsui.Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };

            var tileLayer = OpenStreetMap.CreateTileLayer();

            map.Layers.Add(tileLayer);
            map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(map) 
            { 
                TextAlignment = Mapsui.Widgets.Alignment.Center, 
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, 
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom 
            });

            var location = await Geolocation.GetLocationAsync();
            var smc = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);

            var mapResolutions = map.Resolutions;
            map.Home = n => n.NavigateTo(smc, map.Resolutions[17]);

            mapView.Map = map;
        }

        private async void mapView_MapClicked(object sender, Mapsui.UI.Forms.MapClickedEventArgs e)
        {
            await DisplayAlert("Info", $"Coordinates: LOngitude: {e.Point.Longitude}, Latitude: {e.Point.Latitude}", "OK");
        }
    }
}