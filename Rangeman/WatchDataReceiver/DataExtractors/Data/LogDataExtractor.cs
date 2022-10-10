using Java.Nio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rangeman.DataExtractors.Data
{
    internal class LogDataExtractor : IDataExtractor
    {
        private byte[] data;
        private readonly LogHeaderDataInfo logHeaderDataInfo;

        public LogDataExtractor(LogHeaderDataInfo logHeaderDataInfo)
        {
            this.logHeaderDataInfo = logHeaderDataInfo;
        }

        public void SetData(byte[] data)
        {
            this.data = data;
        }

        public LogData GetLogData(int i)
        {
            int i2 = logHeaderDataInfo.DataSize * i;
            int i3 = (data[i2 + 1] & 255) | ((data[i2 + 2] & 255) << 8);
            int i4 = data[i2 + 7] & 255;
            int i6 = i2 + 28;
            int i7 = i2 + 30;

            var latitude = BitConverter.ToDouble(data, i2 + 8);
            var longitude = BitConverter.ToDouble(data, i2 + 16);            

            Debug.WriteLine($"- LogDataExtractor latitude = {latitude}, longitude = {longitude}");

            var year = i3;
            var month = (data[i2 + 3] & 255);
            var day = data[i2 + 4] & 255;
            var hour = data[i2 + 5] & 255;
            var minute = data[i2 + 6] & 255;
            var second = i4;

            Debug.WriteLine($"- LogDataExtractor Before setting date: year= {year}, month= {month}, day={day}, hour={hour}, minute={minute}, second={second}");

            var date = new DateTime(year, month, day, hour, minute, second);

            var pressure = 0;
            if ((sbyte)data[i6] != -1 || (sbyte)data[i2 + 29] != 127)
            {
                pressure = BitConverter.ToUInt16(data, i6) & 65535;
                Debug.WriteLine($"- LogDataExtractor Pressure: {pressure}");
            }

            var temperature = 0;
            if ((sbyte)data[i7] != -1 || (sbyte)data[i2 + 31] != 127)
            {
                temperature = BitConverter.ToUInt16(data, i7);
                Debug.WriteLine($"- LogDataExtractor Temperaure: {temperature}");
            }

            return new LogData { Latitude = latitude, Longitude = longitude, Date = date, Pressure = pressure, Temperature = temperature };
        }

        public List<LogData> GetAllLogData()
        {
            List<LogData> result = new List<LogData>();

            for(int i=0;i<logHeaderDataInfo.DataCount;i++)
            {
                result.Add(GetLogData(i));
            }

            return result;
        }
    }
}