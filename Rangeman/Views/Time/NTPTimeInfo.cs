using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rangeman.Views.Time
{
    public class NTPTimeInfo : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private bool formIsInvalid = false;
        private string progressMessage;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid NTP server URL")]
        [Display(ShortName = "NTP server's URL")]
        public string NTPServer { get; set; }

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
        public bool HasErrors => formIsInvalid;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if(propertyName == nameof(NTPServer))
            {
                if(string.IsNullOrWhiteSpace(this.NTPServer))
                {
                    formIsInvalid = true;
                    return string.Empty;
                }
                else
                {
                    formIsInvalid = false;
                    return null;
                }
            }

            return null;
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
    }
}