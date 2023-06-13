using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Rangeman.Views.Time
{
    public class NTPTimeInfo : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string progressMessage;
        private string ntpServer;
        private int? secondsCompensation;
        private Dictionary<string, List<string>> propErrors = new Dictionary<string, List<string>>();

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid NTP server URL")]
        [Display(ShortName = "NTP server's URL")]
        public string NTPServer 
        {
            get => ntpServer;
            set
            {
                if(value != this.ntpServer)
                {
                    this.ntpServer = value;
                    this.RaisePropertyChanged(nameof(NTPServer));
                    this.RaiseErrorChanged(nameof(NTPServer));
                }
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Pls. enter a number between -20 and 20")]
        [Display(ShortName = "Seconds compensation")]
        public int? SecondsCompensation
        {
            get => secondsCompensation;
            set
            {
                if (value != this.secondsCompensation)
                {
                    this.secondsCompensation = value;
                    this.RaisePropertyChanged(nameof(SecondsCompensation));
                    this.RaiseErrorChanged(nameof(SecondsCompensation));
                }
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
                var propErrorsCount = this.propErrors.Values.FirstOrDefault(r => r.Count > 0);
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == null)
            {
                return null;
            }

            var errors = new List<string>();

            if (propertyName == nameof(NTPServer))
            {
                if (this.NTPServer == null)
                {
                    return string.Empty;
                }

                errors = ValidateNTPServer(propertyName, this.NTPServer);
            }
            else if (propertyName == nameof(SecondsCompensation))
            {
                if (this.SecondsCompensation == null)
                {
                    return string.Empty;
                }

                errors = ValidateSecondsCompensation(propertyName, this.SecondsCompensation);
            }

            if (this.propErrors.TryGetValue(propertyName, out errors))
            {
                return errors;
            }

            return null;
        }

        private List<string> ValidateSecondsCompensation(string propertyName, int? secondsCompensation)
        {
            List<string> errors;
            if (secondsCompensation < -20 || secondsCompensation > 20)
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "Value should be between -20 and 20 secs."
                    };
                    propErrors.Add(propertyName, errors);
                }
            }
            else if (propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                propErrors.Remove(propertyName);
            }

            return errors;
        }

        private List<string> ValidateNTPServer(string propertyName, string nTPServer)
        {
            List<string> errors;
            if (string.IsNullOrWhiteSpace(nTPServer))
            {
                if (!this.propErrors.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>
                    {
                        "Value should contain valid non-whitespace characters."
                    };
                    propErrors.Add(propertyName, errors);
                }
            }
            else if (propErrors.TryGetValue(propertyName, out errors))
            {
                errors.Clear();
                propErrors.Remove(propertyName);
            }

            return errors;
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