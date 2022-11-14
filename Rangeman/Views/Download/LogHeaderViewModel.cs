using System;

namespace Rangeman
{
    public class LogHeaderViewModel
    {
        public DateTime HeaderTime { get; set; }
        public int OrdinalNumber { get; set; }
        public int DataSize { get; set; }
        public int DataCount { get; set; }
        public int LogAddress { get; set; }
        public int LogTotalLength { get; set; }
        public string Label 
        { 
            get 
            {
                return $"{OrdinalNumber}. {HeaderTime.Year} - {HeaderTime.Month} - {HeaderTime.Day}";
            } 
        }
    }
}