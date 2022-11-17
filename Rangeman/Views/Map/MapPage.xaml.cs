using BruTile.MbTiles;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using Rangeman.Views.Map;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Point = Mapsui.Geometries.Point;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage, IMapPageView
    {
        private const string LinesLayerName = "LinesBetweenPins";

        public MapPage()
        {
            InitializeComponent();

            InitializeMap();
        }

        private async void InitializeMap()
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



        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location != null)
            {
                Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }

            mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude), true);
        }

        #region Pin related methods
        private void mapView_MapClicked(object sender, MapClickedEventArgs e)
        {
            var pinTitle = GetPinTitle(e.Point.Longitude, e.Point.Latitude);

            if (pinTitle == null)
            {
                return;
            }

            ShowPinOnMap(pinTitle, e.Point);
        }

        /// <summary>
        /// Creates a layer with lines between the pin points
        /// </summary>
        /// <param name="name"></param>
        /// <param name="geoWaypoints"></param>
        /// <returns></returns>
        public void AddLinesBetweenPinsAsLayer()
        {
            mapView.Map.Layers.Remove((layer) => layer.Name == LinesLayerName);

            var points = new List<Point>();
            foreach (var wp in ViewModel.NodesViewModel.GetLineConnectableCoordinatesFromStartToGoal())
            {
                if (wp.Longitude != 0 && wp.Latitude != 0)
                {
                    points.Add(SphericalMercator.FromLonLat(wp.Longitude, wp.Latitude));
                }
            }

            Feature lineStringFeature = new Feature()
            {
                Geometry = new LineString(points)
            };

            IStyle linestringStyle = new VectorStyle()
            {
                Fill = null,
                Outline = null,
                Line = { Color = Mapsui.Styles.Color.FromString("Blue"), Width = 4 }
            };

            lineStringFeature.Styles.Add(linestringStyle);

            MemoryProvider memoryProvider = new MemoryProvider(lineStringFeature);

            var linesLayer = new MemoryLayer
            {
                DataSource = memoryProvider,
                Name = LinesLayerName,
                Style = null
            };

            mapView.Map.Layers.Add(linesLayer);
        }

        private string GetPinTitle(double longitude, double latitude)
        {
            var pinTitle = ViewModel.NodesViewModel.AddNodeToMap(longitude, latitude);
            return pinTitle;
        }

        private void ShowPinOnMap(string pinTitle, Position p, bool setTitleImmediately = false)
        {
            var pin = new Pin(mapView)
            {
                Label = "unset",
                Position = p,
                RotateWithMap = false
            };

            pin.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Single;
            
            if(setTitleImmediately)
            {
                pin.Callout.Title = pinTitle;
            }

            pin.Callout.CalloutClicked += (s, e) =>
            {
                Debug.WriteLine($"Map page: entering callout clicked");
                if (e.Callout.Title == "unset")
                {
                    Debug.WriteLine($"Map page: callout clicked. Number of taps: {e.NumOfTaps}");
                    pin.Callout.Title = pinTitle;
                    e.Handled = true;
                    e.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Single;
                }
                else
                {
                    if (BindingContext is MapPageViewModel mapPageViewModel)
                    {
                        Debug.WriteLine("Map: Callout click - other");
                        e.Handled = true;
                        var pinTitle = e.Callout.Title;
                        mapPageViewModel.NodesViewModel.SelectNodeForDeletion(pinTitle, e.Point.Longitude, e.Point.Latitude);
                        mapPageViewModel.ProgressMessage = $"Selected node: {pinTitle} Please use the delete button to delete it.";
                        mapView.SelectedPin = e.Callout.Pin;
                    }
                }
            };

            mapView.Pins.Add(pin);
            pin.ShowCallout();
            AddLinesBetweenPinsAsLayer();
        }

        public void PlaceOnMapClicked(Position p)
        {
            var pinTitle = GetPinTitle(p.Longitude, p.Latitude);

            if (pinTitle == null)
            {
                return;
            }

            ShowPinOnMap(pinTitle, p, true);
        }

        public void RemoveSelectedPin()
        {
            mapView.Pins.Remove(mapView.SelectedPin);
        }
        #endregion

        #region MBTiles related methods

        public void UpdateMapToUseMbTilesFile()
        {
            var map = new Mapsui.Map();
            var fileName = "map.mbtiles";
            var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            string filePath = System.IO.Path.Combine(path, fileName);

            var mbTilesLayer = CreateMbTilesLayer(filePath, "regular");
            map.Layers.Add(mbTilesLayer);

            mapView.Map = map;
        }

        private static TileLayer CreateMbTilesLayer(string path, string name)
        {
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var mbTilesLayer = new TileLayer(mbTilesTileSource) { Name = name };
            return mbTilesLayer;
        }
        #endregion

        Task IMapPageView.DisplayAlert(string title, string message, string button)
        {
            return DisplayAlert(title, message, button);
        }

        public MapPageViewModel ViewModel
        {
            get
            {
                var vm = BindingContext as MapPageViewModel;
                return vm;
            }
        }
    }
}