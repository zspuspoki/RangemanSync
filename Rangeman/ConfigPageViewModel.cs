using System.Windows.Input;

namespace Rangeman
{
    internal class ConfigPageViewModel : ViewModelBase
    {
        private bool useMbTilesChecked;
        private ICommand applyCommand;

        public bool UseMbTilesChecked { get => useMbTilesChecked; set { useMbTilesChecked = value; OnPropertyChanged("UseMbTilesChecked"); } }
    }
}