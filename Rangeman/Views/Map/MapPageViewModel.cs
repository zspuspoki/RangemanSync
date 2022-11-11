using Android.Content;
using Mapsui.Projection;
using Mapsui.Utilities;
using Mapsui;
using System;
using Xamarin.Essentials;
using Mapsui.Layers;
using SQLite;
using BruTile.MbTiles;
using Rangeman.Views.Map;
using Xamarin.Forms;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Collections.Generic;
using Brush = Mapsui.Styles.Brush;
using Point = Mapsui.Geometries.Point;
using System.Linq;

namespace Rangeman
{
    internal class MapPageViewModel : ViewModelBase
    {
        private const string LinesLayerName = "LinesBetweenPins";

        private bool addressPanelIsVisible = false;
        private bool progressBarIsVisible;
        private string progressBarPercentageMessage;
        private string progressMessage;
        private double progressBarPercentageNumber;
        private Mapsui.Map map;
        private NodesViewModel nodesViewModel;
        private AddressPanelViewModel addressPanelViewModel;
        private RowDefinitionCollection gridViewRows;

        public MapPageViewModel(Context context)
        {
            Context = context;
            nodesViewModel = new NodesViewModel();
            addressPanelViewModel = new AddressPanelViewModel();

            gridViewRows = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = 0 }
            };
        }

        /// <summary>
        /// Creates a layer with lines between the pin points
        /// </summary>
        /// <param name="name"></param>
        /// <param name="geoWaypoints"></param>
        /// <returns></returns>
        public void AddLinesBetweenPinsAsLayer()
        {
            this.Map.Layers.Remove((layer) => layer.Name == LinesLayerName);

            var points = new List<Point>();
            foreach (var wp in NodesViewModel.GetLineConnectableCoordinatesFromStartToGoal())
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

            this.map.Layers.Add(linesLayer);
        }

        public void UpdateMapToUseMbTilesFile()
        {
            var map = new Mapsui.Map();
            var fileName = "map.mbtiles";
            var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            string filePath = System.IO.Path.Combine(path, fileName);

            var mbTilesLayer = CreateMbTilesLayer(filePath, "regular");
            map.Layers.Add(mbTilesLayer);

            this.map = map;
        }

        public bool ToggleAddressPanelVisibility()
        {
            if (!addressPanelIsVisible)
            {
                GridViewRows = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                };
            }
            else
            {
                GridViewRows = new RowDefinitionCollection
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                    new RowDefinition { Height = 0 }
                };
            }

            addressPanelIsVisible = !addressPanelIsVisible;

            return addressPanelIsVisible;
        }

        private async void InitializeMap()
        {
            map = new Mapsui.Map
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
        }

        private static TileLayer CreateMbTilesLayer(string path, string name)
        {
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var mbTilesLayer = new TileLayer(mbTilesTileSource) { Name = name };
            return mbTilesLayer;
        }

        #region Properties
        public Mapsui.Map Map
        {
            get
            {
                if (map == null)
                {
                    InitializeMap();
                }

                return map;
            }
        }

        public bool ProgressBarIsVisible { get => progressBarIsVisible; set { progressBarIsVisible = value; OnPropertyChanged("ProgressBarIsVisible"); } }
        public string ProgressBarPercentageMessage { get => progressBarPercentageMessage; set { progressBarPercentageMessage = value; OnPropertyChanged("ProgressBarPercentageMessage"); } }
        public double ProgressBarPercentageNumber { get => progressBarPercentageNumber; set { progressBarPercentageNumber = value; OnPropertyChanged("ProgressBarPercentageNumber"); } }
        public string ProgressMessage { get => progressMessage; set { progressMessage = value; OnPropertyChanged("ProgressMessage"); } }
        public Context Context { get; }
        public NodesViewModel NodesViewModel { get => nodesViewModel; }
        public RowDefinitionCollection GridViewRows { get => gridViewRows ; set { gridViewRows = value; OnPropertyChanged("GridViewRows"); } }
        public AddressPanelViewModel AddressPanelViewModel { get => addressPanelViewModel; }
        #endregion
    }
}