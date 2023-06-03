using System.ComponentModel.DataAnnotations;

namespace Rangeman.Views.Time
{
    public enum MonthType
    {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public enum DayOfWeekType
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    public class CustomTimeInfo
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid year between 2000 and 2200.")]
        public int? Year { get; set; }

        public MonthType Month { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid day of month (1-31)")]
        public int? Day { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid hour (0-23)")]
        public int? Hour { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid minute (0-59)")]
        public int? Minute { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid second (0-59)")]
        public int? Second { get; set; }

        [Display(ShortName = "Day of week")]
        public DayOfWeekType DayOfWeek { get; set; }
    }
}