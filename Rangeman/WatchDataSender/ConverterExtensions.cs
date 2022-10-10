using System;
using System.Collections.Generic;

namespace Rangeman.WatchDataSender
{
    internal static class ConverterExtensions
    {
        public static byte[] ToHeaderByteArray(this MapPageViewModel mapPageViewModel)
        {
            List<byte[]> resultList = new List<byte[]>();

            var infoBytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 9 bytes

            var emptyData = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}; // 15 bytes

            resultList.Add(infoBytes);
            resultList.Add(emptyData);

            var currentTime = DateTime.Now;

            resultList.Add(BitConverter.GetBytes(currentTime.Year));  // 2 bytes
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

            return Utils.GetAllDataArray(resultList);
        }

        public static byte[] ToDataByteArray(this MapPageViewModel mapPageViewModel)
        {
            List<byte[]> resultList = new List<byte[]>();
            
            foreach(var gpsCoordinatePair in mapPageViewModel.GpsCoordinates)
            {
                resultList.Add(BitConverter.GetBytes(gpsCoordinatePair.Longitude));
                resultList.Add(BitConverter.GetBytes(gpsCoordinatePair.Latitude));
            }

            var currentLength = resultList.Count * 8;

            if(currentLength < 256)
            {
                var paddedCount = (256 - currentLength) / 16;

                for(var i=0; i< paddedCount;i++)
                {
                    var paddingBytes = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                                                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}; // 16 bytes
                    resultList.Add(paddingBytes);
                }
            }

            return Utils.GetAllDataArray(resultList);
        }
    }
}