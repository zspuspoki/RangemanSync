using System;

namespace Rangeman
{
    internal class LogHeaderViewModel
    {
        public DateTime HeaderTime { get; set; }
        public int OrdinalNumber { get; set; }
        public string Label 
        { 
            get 
            {
                return $"{OrdinalNumber}. {HeaderTime.Year} - {HeaderTime.Month} - {HeaderTime.Day}";
            } 
        }
    }
}