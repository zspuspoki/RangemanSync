using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rangeman.Views.Time
{
    public enum MonthType
    {
        January = 1,
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
        Monday = 1,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday
    }

    public class CustomTimeInfo : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields
        private int? year;

        private MonthType month;

        private int? day;

        private int? hour;

        private int? minute;

        private int? second;

        private DayOfWeekType dayOfWeek;

        private string progressMessage;
        #endregion

        #region Public properties

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid year between 2000 and 2200.")]
        public int? Year 
        {
            get => year;
            set
            {
                if(value != this.year)
                {
                    this.year = value;
                    this.RaisePropertyChanged("Year");
                    this.RaiseErrorChanged("Year");
                }
            }
        }

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

        [Display(AutoGenerateField = false)]
        public string ProgressMessage 
        { 
            get => this.progressMessage;
            set 
            {
                this.progressMessage = value;
                this.RaisePropertyChanged("ProgressMessage");
            } 
        }

        [Display(AutoGenerateField = false)]
        public bool HasErrors => throw new NotImplementedException();

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Occurs when propery value is changed.
        /// </summary>
        /// <param name="propName">Property name</param>
        private void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        /// <summary>
        /// Occurs when error value is changed.
        /// </summary>
        /// <param name="propName">Property name</param>
        private void RaiseErrorChanged(string propName)
        {
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propName));
            }
        }
    }
}