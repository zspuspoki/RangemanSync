using Rangeman.Services.BackgroundTimeSyncService;
using Rangeman.Services.SharedPreferences;
using Rangeman.Views.Time;
using System;
using Xamarin.Forms;

namespace RangemanSync.Android.Services.BackgroundTimeSync
{
    public class TimeSyncServiceStatus : ITimeSyncServiceStatus
    {
        private readonly ISharedPreferencesService sharedPreferencesService;
        private TimeSyncServiceState state;

        public TimeSyncServiceStatus(ISharedPreferencesService sharedPreferencesService)
        {
            this.sharedPreferencesService = sharedPreferencesService;

            var savedState = sharedPreferencesService.GetValue(Rangeman.Constants.ServiceState, "Closed");

            if(Enum.TryParse(typeof(TimeSyncServiceState), savedState, out var savedStateResult))
            {
                this.state = (TimeSyncServiceState)savedStateResult;
            }
        }

        public TimeSyncServiceState GetState()
        {
            return state;
        }

        public void SetState(TimeSyncServiceState state)
        {
            this.state = state;

            sharedPreferencesService.SetValue(Rangeman.Constants.ServiceState, state.ToString());

            MessagingCenter.Send<ITimeSyncServiceStatus>(this, TimeSyncServiceMessages.ServiceStateChanged.ToString());
        }
    }
}