using System.ComponentModel;
using Xamarin.Forms;

namespace Rangeman.Views.Time
{
    public class NTPTimeViewModel
    {
        private NTPTimeInfo ntpTimeInfo;

        public NTPTimeViewModel()
        {
            this.ntpTimeInfo = new NTPTimeInfo();
            this.CommitCommand = new Command<object>(this.OnCommit);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NTPTimeInfo NTPTimeInfo
        {
            get => ntpTimeInfo;
            set
            {
                this.ntpTimeInfo = value;
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