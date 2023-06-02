using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class CustomTimeViewModel : INotifyPropertyChanged
    {
        private CustomTimeInfo customTimeInfo;

        public CustomTimeViewModel()
        {
            this.customTimeInfo = new CustomTimeInfo();
            this.CommitCommand = new Command<object>(this.OnCommit);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CustomTimeInfo CustomTimeInfo
        {
            get => customTimeInfo;
            set
            {
                this.customTimeInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets an ICommand implementation wrapping a commit action.
        /// </summary>
        public Command<object> CommitCommand { get; set; }

        private void OnCommit(object dataForm)
        {
            
        }
    }
}