using Android.Gms.Maps.Model;
using Mapsui.Projection;
using Microsoft.Extensions.Logging;
using Rangeman.Common;
using Rangeman.Views.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Essentials;

namespace Rangeman.WatchDataSender
{
    internal class MapPageDataConverter
    {
        private readonly NodesViewModel nodesViewModel;
        private List<byte[]> transitPointsWithInterimPoints;
        private ILogger<MapPageDataConverter> logger;

        public MapPageDataConverter(NodesViewModel nodesViewModel, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MapPageDataConverter>();
            this.nodesViewModel = nodesViewModel;
            transitPointsWithInterimPoints = GetTransitPointsWithInterimPoints();
        }

        public byte[] GetHeaderByteArray()
        {
            List<byte[]> resultList = new List<byte[]>();

            //00-06-00-00-00-1C-00-00-00
            //In the above example, the 6 is TransitPointCount, 1C (dec 28) is the NodeCount
            //2nd byte : TransitPointCount
            //6th byte: NodeCount

            logger.LogDebug($"--- GetHeaderByteArray: transitPointsWithInterimPoints.Count = {transitPointsWithInterimPoints.Count}");

            var nodeCount = (byte)(transitPointsWithInterimPoints.Count >> 1);
            var transitPointCount = (byte)nodesViewModel.GetTransitPointCoordinates().Count();

            var infoBytes = new byte[] { 0, transitPointCount, 0, 0, 0, nodeCount, 0, 0, 0 }; // 9 bytes

            var emptyData = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}; // 15 bytes

            resultList.Add(infoBytes);
            resultList.Add(emptyData);

            var currentTime = DateTime.Now;

            resultList.Add(BitConverter.GetBytes((ushort)currentTime.Year));  // 2 bytes
            resultList.Add(new byte[] { (byte)currentTime.Month });   // 1
            resultList.Add(new byte[] { (byte)currentTime.Day });     // 1
            resultList.Add(new byte[] { (byte)currentTime.Hour });    // 1
            resultList.Add(new byte[] { (byte)currentTime.Minute });  // 1
            resultList.Add(new byte[] { (byte)currentTime.Second });  // 1, Total = 7 bytes

            for (var i = 0; i < 75; i++)  // Total length should be 256, so 225 bytes are missing. 225 / 3 = 75
            {
                var bytesToAdd = new byte[] { 0xff, 0xff, 0xff };
                resultList.Add(bytesToAdd);
            }

            var result = Utils.GetAllDataArray(resultList);

            logger.LogDebug($"--- Header byte array from converter : {Utils.GetPrintableBytesArray(result)}");

            return result;
        }

        public byte[] GetDataByteArray()
        {
            List<byte[]> resultList = new List<byte[]>();
            
            foreach(var gpsCoordinatePair in nodesViewModel.GetStartEndCoordinates())
            {
                resultList.Add(BitConverter.GetBytes(gpsCoordinatePair.Latitude));
                resultList.Add(BitConverter.GetBytes(gpsCoordinatePair.Longitude));
            }

            foreach(var gpsCoordinatePair in nodesViewModel.GetTransitPointCoordinates())
            {
                resultList.Add(BitConverter.GetBytes(gpsCoordinatePair.Latitude));
                resultList.Add(BitConverter.GetBytes(gpsCoordinatePair.Longitude));
            }

            foreach(var gpsCoordinateElement in transitPointsWithInterimPoints)
            {
                resultList.Add(gpsCoordinateElement);
            }

            var currentLength = resultList.Count * 8;
            var fullDataSectionLength = 256; // The data array should be 256 * 1, 256 * 2, 256 * n bytes long, so padding should be calculated here

            while(currentLength > fullDataSectionLength)
            {
                fullDataSectionLength += 256;
            }

            if(currentLength < fullDataSectionLength)
            {
                var paddedCount = (fullDataSectionLength - currentLength) / 16;

                for (var i=0; i< paddedCount;i++)
                {
                    var paddingBytes = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}; // 16 bytes
                    resultList.Add(paddingBytes);
                }
            }

            var result = Utils.GetAllDataArray(resultList);

            logger.LogDebug($"--- Data byte array from converter : {Utils.GetPrintableBytesArray(result)}");

            return result;
        }

        /// <summary>
        /// This is from logcat_write4
        /// </summary>
        /// <param name="mapPageViewModel"></param>
        /// <returns></returns>
        public byte[] GetTestHeaderByteArray()
        {
            var result = new List<byte>() { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xE6, 0x07, 0x0A, 0x04, 0x05, 0x18, 0x1A, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            while (result.Count < 256)
            {
                result.Add(0xff);
            }

            return result.ToArray();
        }

        /// <summary>
        /// This is from logcat_write4
        /// </summary>
        /// <param name="mapPageViewModel"></param>
        /// <returns></returns>
        public byte[] GetTestDataByteArray()
        {
            var result = new List<byte>() { 0xA5, 0x36, 0x7D, 0x76, 0x28, 0xBF, 0x47, 0x40, 0x42, 0x88, 0xF6, 0x73, 0xAA, 0x03, 0x33, 0x40, 0xB9, 0xA3, 0x35, 0xF7, 0x22, 0xBF, 0x47, 0x40, 0x53, 0x4F, 0x7F, 0x27, 0xCB, 0x03, 0x33, 0x40, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            
            while(result.Count < 256)
            {
                result.Add(0xff);
            }

            return result.ToArray();
        }

        private List<byte[]> GetTransitPointsWithInterimPoints()
        {
            var result = new List<byte[]>();
            var transitPointCount = nodesViewModel.GetTransitPointCoordinates().Count;

            if (transitPointCount > 0)
            {
                var startEndCoordinates = nodesViewModel.GetStartEndCoordinates().ToList();
                var transitPoints = nodesViewModel.GetTransitPointCoordinates().ToList();
                var firstElement = startEndCoordinates[0]; //S

                for (var i = 0; i <= transitPoints.Count; i++)
                {
                    var endCoordinate = i < transitPoints.Count ? transitPoints[i] : startEndCoordinates[1]; //1 

                    var interimCoordinates = transitPointCount <= 4 ? 
                        GetInterimGpsCoordinates(firstElement, endCoordinate) : 
                        GetInterimGpsCoordinates(firstElement, endCoordinate, 2); //A  

                    foreach (var gpsCOordinateElement in interimCoordinates)
                    {
                        result.Add(gpsCOordinateElement);
                    }

                    if (i < transitPoints.Count) // The last G point should not be added to transit&interim list
                    {
                        result.Add(BitConverter.GetBytes(endCoordinate.Latitude));
                        result.Add(BitConverter.GetBytes(endCoordinate.Longitude));
                    }

                    firstElement = endCoordinate;
                }
            }

            //1 - A - 

            return result;
        }

        private List<byte[]> GetInterimGpsCoordinates(GpsCoordinatesViewModel startCoordinate, GpsCoordinatesViewModel endCoordinate)
        {
            logger.LogDebug($"-- MapPageDataConverter: Calculating interim points. Coor1: ({startCoordinate.Latitude}, {startCoordinate.Longitude}) Coor2: ({endCoordinate.Latitude}, {endCoordinate.Longitude})");

            var result = new List<byte[]>();
            var latLngBoundsBuilder =
            new LatLngBounds.Builder()
                .Include(new LatLng(startCoordinate.Latitude, startCoordinate.Longitude))
                .Include(new LatLng(endCoordinate.Latitude, endCoordinate.Longitude)).Build();

            var centerCoodinates = latLngBoundsBuilder.Center;

            result.Add(BitConverter.GetBytes(centerCoodinates.Latitude));
            result.Add(BitConverter.GetBytes(centerCoodinates.Longitude));

            logger.LogDebug($"-- MapPageDataConverter:Calculted interim point: ({centerCoodinates.Latitude}, {centerCoodinates.Longitude})");

            return result;
        }

        private List<byte[]> GetInterimGpsCoordinates(GpsCoordinatesViewModel startCoordinate, GpsCoordinatesViewModel endCoordinate, int pointCount)
        {
            logger.LogDebug($"-- MapPageDataConverter: GetInterimGpsCoordinates (multipoint) Calculating interim points. Coor1: ({startCoordinate.Latitude}, {startCoordinate.Longitude}) Coor2: ({endCoordinate.Latitude}, {endCoordinate.Longitude})");

            if (pointCount < 0)
            {
                throw new ArgumentNullException("pointCount cannot be less than 0");
            }

            var result = new List<byte[]>();

            var displacement = Location.CalculateDistance(startCoordinate.Latitude, startCoordinate.Longitude, 
                endCoordinate.Latitude, endCoordinate.Longitude, DistanceUnits.Kilometers);

            var distanceBetweenPoints = displacement / (pointCount + 1);

            for (var i = 1; i <= pointCount; i++)
            {
                var t = (distanceBetweenPoints * i) / displacement;

                var latitude = (1 - t) * startCoordinate.Latitude + t * endCoordinate.Latitude;
                var longitude = (1 - t) * startCoordinate.Longitude + t * endCoordinate.Longitude;

                logger.LogDebug($"-- MapPageDataConverter:Calculted interim point (multipoint): ({latitude}, {longitude})");

                result.Add(BitConverter.GetBytes(latitude));
                result.Add(BitConverter.GetBytes(longitude));
            }

            return result;
        }
    }
}