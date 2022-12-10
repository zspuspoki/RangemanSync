using Java.Nio;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rangeman.DataExtractors.Data
{
    internal class LogDataExtractor : IDataExtractor
    {
        private byte[] data;
        private readonly int headerDataSize;
        private readonly int headerDataCount;
        private ILogger<LogDataExtractor> logger;

        public LogDataExtractor(int headerDataSize, int headerDataCount, ILoggerFactory loggerFactory)
        {
            this.headerDataSize = headerDataSize;
            this.headerDataCount = headerDataCount;
            this.logger = loggerFactory.CreateLogger<LogDataExtractor>();
        }

        public void SetData(byte[] data)
        {
            this.data = data;
        }

        public LogData GetLogData(int i)
        {
            int i2 = headerDataSize * i;
            int i3 = (data[i2 + 1] & 255) | ((data[i2 + 2] & 255) << 8);
            int i4 = data[i2 + 7] & 255;
            int i6 = i2 + 28;
            int i7 = i2 + 30;

            var latitude = BitConverter.ToDouble(data, i2 + 8);
            var longitude = BitConverter.ToDouble(data, i2 + 16);            

            logger.LogDebug($"- LogDataExtractor latitude = {latitude}, longitude = {longitude}");

            if(double.IsNaN(latitude) || double.IsNaN(longitude))
            {
                logger.LogDebug("- LogDataExtractor : Longitude or Latitude is NaN. Exiting");
                return null;
            }

            var year = i3;
            var month = (data[i2 + 3] & 255);
            var day = data[i2 + 4] & 255;
            var hour = data[i2 + 5] & 255;
            var minute = data[i2 + 6] & 255;
            var second = i4;

            logger.LogDebug($"- LogDataExtractor Before setting date: year= {year}, month= {month}, day={day}, hour={hour}, minute={minute}, second={second}");

            if(year == 0 || month == 0 || day == 0)
            {
                return null;
            }

            var date = new DateTime(year, month, day, hour, minute, second);

            var pressure = 0;
            if ((sbyte)data[i6] != -1 || (sbyte)data[i2 + 29] != 127)
            {
                pressure = BitConverter.ToUInt16(data, i6) & 65535;
                logger.LogDebug($"- LogDataExtractor Pressure: {pressure}");
            }

            var temperature = 0;
            if ((sbyte)data[i7] != -1 || (sbyte)data[i2 + 31] != 127)
            {
                temperature = BitConverter.ToUInt16(data, i7);
                logger.LogDebug($"- LogDataExtractor Temperature: {temperature}");
            }

            return new LogData { Latitude = latitude, Longitude = longitude, Date = date, Pressure = pressure, Temperature = temperature };
        }

        public List<LogData> GetAllLogData()
        {
            if(data.Length == 0)
            {
                logger.LogDebug("-  LogDataExtractor: the data length is 0, so most probably the data transmission ended without receiving all data. Returning null");
                return null;
            }

            List<LogData> result = new List<LogData>();

            logger.LogDebug($"Before the for loop in GetAllLogData(). headerDataCount = {headerDataCount}");

            for(int i=0;i<headerDataCount;i++)
            {
                try
                {
                    var logData = GetLogData(i);

                    if (logData != null)
                    {
                        result.Add(logData);
                    }
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occured during adding LogData, skipping this entry.");
                }
            }

            return result;
        }
    }
}