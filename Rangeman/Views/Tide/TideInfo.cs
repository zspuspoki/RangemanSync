using Rangeman.Views.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Rangeman.Views.Tide
{
    public class TideInfo: BaseDomainInfo, INotifyDataErrorInfo
    {
        #region Fields
        private int? year;

        private MonthType month;

        private int? day;

        private int? hour;

        private int? minute;

        private string gpsCoordinates;

        private readonly ITimeInfoValidator timeInfoValidator;
        #endregion

        public TideInfo(ITimeInfoValidator timeInfoValidator)
        {
            this.timeInfoValidator = timeInfoValidator;
        }

        #region Public properties
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid year between 2000 and 2200.")]
        public int? Year
        {
            get => year;
            set
            {
                if (value != this.year)
                {
                    this.year = value;
                    this.RaisePropertyChanged(nameof(Year));
                    this.RaiseErrorChanged(nameof(Year));
                }
            }
        }

        public MonthType Month
        {
            get => month;
            set
            {
                if (value != this.month)
                {
                    this.month = value;
                    this.RaisePropertyChanged(nameof(Month));
                    this.RaiseErrorChanged(nameof(Month));
                }
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid day of month (1-31)")]
        public int? Day
        {
            get => day;
            set
            {
                if (value != this.day)
                {
                    this.day = value;
                    this.RaisePropertyChanged(nameof(Day));
                    this.RaiseErrorChanged(nameof(Day));
                }
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid hour (0-23)")]
        public int? Hour
        {
            get => hour;
            set
            {
                this.hour = value;
                this.RaisePropertyChanged(nameof(Hour));
                this.RaiseErrorChanged(nameof(Hour));
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid minute (0-59)")]
        public int? Minute
        {
            get => minute;
            set
            {
                this.minute = value;
                this.RaisePropertyChanged(nameof(Minute));
                this.RaiseErrorChanged(nameof(Minute));
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter valid GPS coordinates (latitude, longitude)")]
        public string GPSCoordinates
        {
            get => gpsCoordinates;
            set
            {
                this.gpsCoordinates = value;
                this.RaisePropertyChanged(nameof(GPSCoordinates));
                this.RaiseErrorChanged(nameof(GPSCoordinates));
            }
        }

        [Display(AutoGenerateField = false)]
        public bool HasErrors
        {
            get
            {
                var propErrorsCount = this.timeInfoValidator.PropErrors.Values.FirstOrDefault(r => r.Count > 0);
                if (propErrorsCount != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            throw new NotImplementedException();
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