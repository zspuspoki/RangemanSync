
using System;

namespace Rangeman.DataExtractors.Data
{
    internal class LogData
    {
        public DateTime Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Pressure { get; set; }
        public long Temperature { get; set; }
    }
}