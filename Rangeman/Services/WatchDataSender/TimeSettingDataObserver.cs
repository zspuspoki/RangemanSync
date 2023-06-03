using Android.Gms.Tasks;
using nexus.protocols.ble;
using System;
using System.Threading.Tasks;

namespace Rangeman.Services.WatchDataSender
{
    public class TimeSettingDataObserver : IObserver<Tuple<Guid, byte[]>>
    {
        public enum State
        {
            DstWatchState,
            DstSetting0,
            DstSetting1,
            GetWorldCities0,
            GetWorldCities1
        }

        private State CurrentState = State.DstWatchState;
        private readonly IBleGattServerConnection gattServer;
        private readonly TaskCompletionSource<bool> initPhaseIsReadyTaskCmpSource;

        public TimeSettingDataObserver(IBleGattServerConnection gattServer, TaskCompletionSource<bool> initPhaseIsReadyTaskCmpSource)
        {
            this.gattServer = gattServer;
            this.initPhaseIsReadyTaskCmpSource = initPhaseIsReadyTaskCmpSource;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public async void OnNext(Tuple<Guid, byte[]> value)
        {
            //Write
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioAllFeaturesCharacteristic), value.Item2);

            switch (CurrentState)
            {
                case State.DstWatchState:
                    //Read
                    CurrentState = State.DstSetting0;
                    await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioReadRequestForAllFeaturesCharacteristic), new byte[] { 0x1e, 0 });
                    break;

                case State.DstSetting0:
                    //Read
                    CurrentState = State.DstSetting1;
                    await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioReadRequestForAllFeaturesCharacteristic), new byte[] { 0x1e, 1 });
                    break;

                case State.DstSetting1:
                    //Read
                    CurrentState = State.GetWorldCities0;
                    await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioReadRequestForAllFeaturesCharacteristic), new byte[] { 0x1f, 0 });
                    break;

                case State.GetWorldCities0:
                    //Read
                    CurrentState = State.GetWorldCities1;
                    await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioReadRequestForAllFeaturesCharacteristic), new byte[] { 0x1f, 1 });
                    break;

                case State.GetWorldCities1:
                    initPhaseIsReadyTaskCmpSource.TrySetResult(true);
                    break;
            }
        }
    }
}