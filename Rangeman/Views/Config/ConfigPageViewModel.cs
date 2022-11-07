using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Rangeman
{
    internal class ConfigPageViewModel : ViewModelBase
    {
        private bool useMbTilesChecked;
        private ICommand applyCommand;

        public event EventHandler UseMbTilesClicked;

        public bool UseMbTilesChecked 
        { 
            get => useMbTilesChecked; 
            set 
            { 
                useMbTilesChecked = value; 
                OnPropertyChanged("UseMbTilesChecked"); 
            } 
        }

        public ICommand ApplyCommand
        {
            get
            {
                if(applyCommand == null)
                {
                    applyCommand = new Command((o) => ApplySettings(), (o) => CanApplySettings());
                }

                return applyCommand;
            }
        }

        private bool CanApplySettings()
        {
            return true;
        }

        private void ApplySettings()
        {
            if (UseMbTilesChecked)
            {
                if (UseMbTilesClicked != null)
                {
                    UseMbTilesClicked(this, new EventArgs());
                }
            }
        }
    }
}