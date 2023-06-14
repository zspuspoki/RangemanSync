using Rangeman.Views.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

        private string cityName;

        private readonly ITimeInfoValidator timeInfoValidator;

        private string progressMessage;
        #endregion

        public TideInfo(ITimeInfoValidator timeInfoValidator)
        {
            this.timeInfoValidator = timeInfoValidator;
        }

        #region Public properties

        [Required(AllowEmptyStrings = false, ErrorMessage = "Plase enter a valid city name. (max. 15 chars)")]
        [Display(ShortName = "City Name")]
        public string CityName
        {
            get => cityName;
            set 
            {
                this.cityName = value;
                this.RaisePropertyChanged(nameof(CityName));
                this.RaiseErrorChanged(nameof(CityName));
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter valid GPS coordinates (latitude, longitude)")]
        [Display(ShortName = "GPS Coordintes")]
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
            if (propertyName == null)
            {
                return null;
            }

            var errors = new List<string>();

            if (propertyName == nameof(Year))
            {
                if (this.Year == null)
                {
                    return string.Empty;
                }

                errors = timeInfoValidator.ValidateYear(propertyName, this.Year);
            }
            else if (propertyName == nameof(Hour))
            {
                if (this.Hour == null)
                {
                    return string.Empty;
                }

                errors = timeInfoValidator.ValidateHour(propertyName, this.Hour);
            }
            else if (propertyName == nameof(Minute))
            {
                if (this.Minute == null)
                {
                    return string.Empty;
                }

                errors = timeInfoValidator.ValidateMinute(propertyName, this.Minute);
            }
            else if (propertyName == nameof(Day))
            {
                if (this.Day == null)
                {
                    return string.Empty;
                }

                errors = timeInfoValidator.ValidateDay(propertyName, this.Day);
            }
            else if(propertyName == nameof(CityName))
            {
                if(this.CityName == null)
                {
                    return string.Empty;
                }

                ValidateCity(propertyName, this.CityName);
            }
            else if(propertyName == nameof(GPSCoordinates))
            {
                if (this.GPSCoordinates == null)
                {
                    return string.Empty;
                }

                ValidateGPSCoordinates(propertyName, this.GPSCoordinates);
            }


            if (this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
            {
                return errors;
            }

            return null;
        }

        public List<string> ValidateGPSCoordinates(string propertyName, string gpsCoordinates)
        {
            List<string> errors;
            if (string.IsNullOrWhiteSpace(gpsCoordinates))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "This should contain valid non-whitespace characters."
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if(!gpsCoordinates.Contains(","))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "This should contain a separator character (,)"
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (gpsCoordinates.Split(",").Length != 2)
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "This should contain the coordinates separated by a , character."
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (!double.TryParse(gpsCoordinates.Split(",")[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "Latitude should be a valid number."
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (!double.TryParse(gpsCoordinates.Split(",")[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var _))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "Longitude should be a valid number."
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (double.TryParse(gpsCoordinates.Split(",")[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude) 
                && (latitude<-90 || latitude > 90))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "Latitude (first number) should be between -90 and 90"
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (double.TryParse(gpsCoordinates.Split(",")[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude) && 
                (longitude < -180 || longitude > 180))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "Longitude (first number) should be between -180 and 180"
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.timeInfoValidator.PropErrors.Remove(propertyName);
            }

            return errors;
        }

        public List<string> ValidateCity(string propertyName, string cityname)
        {
            List<string> errors;
            if(string.IsNullOrWhiteSpace(cityname))
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "City name should contain valid non-whitespace characters."
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (cityname.Length >15)
            {
                if (!this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "City name's max length should exceed 15 characters."
                    };
                    this.timeInfoValidator.PropErrors.Add(propertyName, errors);
                }
            }
            else if (this.timeInfoValidator.PropErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                this.timeInfoValidator.PropErrors.Remove(propertyName);
            }

            return errors;
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