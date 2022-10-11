using nexus.protocols.ble;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rangeman.WatchDataSender
{
    internal class RemoteWatchController
    {
        private readonly IBleGattServerConnection gattServer;

        public RemoteWatchController(IBleGattServerConnection gattServer)
        {
            this.gattServer = gattServer;
        }

        public async Task SendInitCommandsAndWaitForCCCData(byte[] convoyData)
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
            (data) =>
            {
                if (data.SequenceEqual(convoyData))
                {
                    taskCompletionSource.SetResult(data);
                }
            });

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                      Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                                      Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, 0x12, 0x00, 0x00 }); // Category = 18 - connection setup

            await taskCompletionSource.Task;
        }

        public async Task SendConvoyConnectionParameters()
        {
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x01, 0x18, 0x00, 0x18, 0x00, 0x00, 0x00, 0x58, 0x02 });
        }

        public async Task<ConnectionParameters> SendCategoryAndWaitForConnectionParams(byte categoryId)
        {
            var taskCompletionSource = new TaskCompletionSource<ConnectionParameters>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
            (data) =>
            {
                var kindData = data[0];

                if (kindData == 2 || kindData == 6)
                {
                    var connectionParameters = new ConnectionParameters(data);

                    taskCompletionSource.SetResult(connectionParameters);
                }
            });

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, categoryId, 0x00, 0x00 }); // Category = 18 - connection setup

            var result = await taskCompletionSource.Task;
            return result;
        }

        public async Task SendConnectionSettingsBasedOnParams()
        {
            throw new NotImplementedException();
        }
    }
}