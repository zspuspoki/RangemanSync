using Android.Graphics;
using BruTile.MbTiles;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using Microsoft.Extensions.Logging;
using Rangeman.Services.PhoneLocation;
using Rangeman.Services.SharedPreferences;
using Rangeman.Views.Map;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Point = Mapsui.Geometries.Point;

namespace Rangeman
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage, IMapPageView
    {
        private const string LinesLayerName = "LinesBetweenPins";
        private readonly ILogger<MapPage> logger;
        private readonly ILocationService locationService;
        private readonly ISharedPreferencesService sharedPreferencesService;
        private bool previouslyUsedMbTilesHasBeenLoaded = false;

        public MapPage(ILogger<MapPage> logger,ILocationService locationService, ISharedPreferencesService sharedPreferencesService)
        {
            this.logger = logger;
            this.locationService = locationService;
            this.sharedPreferencesService = sharedPreferencesService;
            locationService.GetPhoneLocation();
            InitializeComponent();

            InitProgressLabel();
        }

        private void InitProgressLabel()
        {
            lblProgress.GestureRecognizers.Add(new TapGestureRecognizer
            {
                //TODO: Move this command to the viewmodel
                Command = new Command(()=> 
                {
                    ViewModel.ProgressMessage = "";
                })
            });
        }

        private async Task<bool> InitializeMap(bool forceMapUpdate = false)
        {
            bool hasTileLayer = false;

            if (mapView.Map != null)
            {
                mapView.Map.Layers.ForEach(l => { if (l is TileLayer) hasTileLayer = true; });
            }

            if(!forceMapUpdate && hasTileLayer)
            {
                return false;
            }

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

            var location = locationService.Location;

            if (location != null)
            {
                var smc = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);

                var mapResolutions = map.Resolutions;
                map.Home = n => n.NavigateTo(smc, map.Resolutions[17]);

                mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude), true);
            }

            mapView.Map = map;
            mapView.Map.RotationLock = true;
            return true;
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var mapInitialized = await InitializeMap();

            if (!mapInitialized)
            {
                var location = locationService.Location;

                if (location != null)
                {
                    mapView.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude), true);
                }
            }

            if (!previouslyUsedMbTilesHasBeenLoaded)
            {
                LoadPreviouslyUsedMbTilesFile();
                previouslyUsedMbTilesHasBeenLoaded = true;
            }
        }

        #region Pin related methods
        private void mapView_MapClicked(object sender, MapClickedEventArgs e)
        {
            var pinTitle = AddNodeToViewModelAndGetPinTitle(e.Point.Longitude, e.Point.Latitude);

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
                if (wp.HasValidCoordinates)
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

        private string AddNodeToViewModelAndGetPinTitle(double longitude, double latitude)
        {
            ViewModel.ShowDistanceFromCurrentPosition(longitude, latitude);
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
                logger.LogDebug($"Map page: entering callout clicked");
                if (e.Callout.Title == "unset")
                {
                    logger.LogDebug($"Map page: callout clicked. Number of taps: {e.NumOfTaps}");
                    pin.Callout.Title = pinTitle;
                    e.Handled = true;
                    e.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Single;
                }
                else
                {
                    if (BindingContext is MapPageViewModel mapPageViewModel)
                    {
                        logger.LogDebug("Map: Callout click - other");
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
            if(ViewModel.NodesViewModel.HasTheSameLatitudeLongitudeNode(p.Latitude, p.Longitude))
            {
                ViewModel.ProgressMessage = "Error! The map already has a node with the same latitude - longitude values.";
                return;
            }

            var pinTitle = AddNodeToViewModelAndGetPinTitle(p.Longitude, p.Latitude);

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

        public async void UpdateMapToUseMbTilesFile()
        {
            try
            {
                logger.LogInformation("UpdateMapToUseMbTilesFile started");

                var selectedFile = await SelectMBTilesFile();

                if (selectedFile != null)
                {
                    SetMapSourceBasedOnMbTilesFile(selectedFile.FullPath);

                    sharedPreferencesService.SetValue(Constants.MbTilesFile, selectedFile.FullPath);

                    ViewModel.ProgressMessage = $"Mbtiles based map selection has been completed. {selectedFile.FileName}";
                }
                else
                {
                    logger.LogDebug("selectedFile is null");
                    ViewModel.ProgressMessage = "Map setting was unsuccessful due to the problem with the file selection.";
                }
            }
            catch(Exception ex)
            {
                logger.LogDebug(ex, "Error using UpdateMapToUseMbTilesFile");
                ViewModel.ProgressMessage = "An unexpected error occured during trying to set the map's source using an mbtiles file.";
            }
        }

        public async void UpdateMapToUseWebBasedMbTiles()
        {
            try
            {
                await InitializeMap(true);

                sharedPreferencesService.SetValue(Constants.MbTilesFile, string.Empty);
                
                ViewModel.ProgressMessage = $"Web based mbtiles file selection has been completed.";
            }
            catch(Exception ex)
            {
                logger.LogDebug(ex, "Error using UpdateMapToUseWebBasedMbTiles");
            }
        }

        private void LoadPreviouslyUsedMbTilesFile()
        {
            try
            {
                var fileFullPath = sharedPreferencesService.GetValue(Constants.MbTilesFile, string.Empty);
                var fileName = System.IO.Path.GetFileName(fileFullPath);

                if (!string.IsNullOrWhiteSpace(fileFullPath))
                {
                    SetMapSourceBasedOnMbTilesFile(fileFullPath);

                    ViewModel.ProgressMessage = $"Mbtiles based map selection has been completed. {fileName}";
                }
            }
            catch(Exception ex)
            {
                logger.LogDebug(ex, "Error using LoadPreviouslyUsedMbTilesFile");
                ViewModel.ProgressMessage = "An unexpected error occured during trying to use the previously set mbtiles file. Please check if the file still exists.";
            }
        }

        private void SetMapSourceBasedOnMbTilesFile(string fullPath)
        {
            logger.LogDebug($"Selected file's full path {fullPath}");

            var mbTilesLayer = CreateMbTilesLayer(fullPath, "regular");

            var map = new Mapsui.Map();
            map.Layers.Add(mbTilesLayer);

            mapView.Map = map;
        }

        private async Task<FileResult> SelectMBTilesFile()
        {
            try
            {
                var result = await FilePicker.PickAsync();

                if (result.FileName.EndsWith("mbtiles", StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
                else
                {
                    await DisplayAlert("Error", "Please choose .mbtiles file", "cancel");
                    return null;
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "SelectMBTilesFile error");
            }

            return null;
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

        public void ShowOnMap(Position position)
        {
            var smc = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);
            mapView.Navigator.NavigateTo(smc, mapView.Map.Resolutions[mapView.Map.Resolutions.Count - 1]);
        }

        public void DisplayProgressMessage(string message)
        {
            ViewModel.ProgressMessage = message;
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