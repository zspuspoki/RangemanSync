﻿using Android.Content;
using Mapsui.Projection;
using Mapsui.Utilities;
using Mapsui;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Mapsui.Layers;
using SQLite;
using BruTile.MbTiles;
using AndroidX.Lifecycle;
using System.Diagnostics;
using System.Linq;

namespace Rangeman
{
    internal class MapPageViewModel : ViewModelBase
    {
        public const int MaxNumberOfTransitPoints = 9;

        private List<GpsCoordinatesViewModel> startEndCoordinates = new List<GpsCoordinatesViewModel>();
        private List<GpsCoordinatesViewModel> transitPointCordinates = new List<GpsCoordinatesViewModel>();

        private bool hasStartCoordinate;
        private bool hasEndCoordinate;
        private bool progressBarIsVisible;
        private string progressBarPercentageMessage;
        private string progressMessage;
        private double progressBarPercentageNumber;
        private Mapsui.Map map;

        public MapPageViewModel(Context context)
        {
            Context = context;
        }

        public void AddStartCoordinates(double longitude, double latitude)
        {
            startEndCoordinates.Add(new GpsCoordinatesViewModel { Longitude = longitude, Latitude = latitude });
            hasStartCoordinate = true;
        }

        public void AddEndCoordinates(double longitude, double latitude)
        {
            startEndCoordinates.Add(new GpsCoordinatesViewModel { Longitude = longitude, Latitude = latitude });
            hasEndCoordinate = true;
            HasRoute = true;
        }

        public void AddTransitPointCoordinates(double longitude, double latitude)
        {
            transitPointCordinates.Add(new GpsCoordinatesViewModel { Longitude = longitude, Latitude = latitude });
        }

        public void ResetCoordinates()
        {
            startEndCoordinates.Clear();
            HasRoute = false;
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

        public string ClickOnMap(double longitude, double latitude)
        {
            if (HasEndCoordinate && HasStartCoordinate)
            {
                if (TransitPointCoordinates.Count() == MapPageViewModel.MaxNumberOfTransitPoints)
                {
                    return null;
                }
            }

            string pinTitle;
            if (!HasStartCoordinate)
            {
                pinTitle = "S";
                AddStartCoordinates(longitude, latitude);
            }
            else if (!HasEndCoordinate)
            {
                pinTitle = "E";
                AddEndCoordinates(longitude, latitude);
            }
            else
            {
                var transitPointCoordinateCount = TransitPointCoordinates.Count();

                Debug.WriteLine($"-- mapView_MapClicked: transitPointCoordinateCount = {transitPointCoordinateCount}");

                pinTitle = $"{transitPointCoordinateCount + 1}";
                AddTransitPointCoordinates(longitude, latitude);
            }

            return pinTitle;
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
        public bool HasStartCoordinate => hasStartCoordinate;
        public bool HasEndCoordinate => hasEndCoordinate;
        public IEnumerable<GpsCoordinatesViewModel> StartEndCoordinates => startEndCoordinates;
        public IEnumerable<GpsCoordinatesViewModel> TransitPointCoordinates => transitPointCordinates;
        public bool HasRoute { get; set; }
        #endregion
    }
}