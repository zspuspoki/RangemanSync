using nexus.protocols.ble;
using System;

namespace Rangeman.WatchDataSender
{
    internal class WatchDataSenderService
    {
        private readonly BlePeripheralConnectionRequest connection;
        private readonly byte[] data;
        private readonly byte[] header;

        public WatchDataSenderService(BlePeripheralConnectionRequest connection, byte[] data, byte[] header)
        {
            this.connection = connection;
            this.data = data;
            this.header = header;
        }

        public async void SendRoute()
        {
            var remoteWatchController = new RemoteWatchController(this.connection.GattServer);

            await remoteWatchController.SendInitCommandsAndWaitForCCCData(new byte[] { 00, 00, 00 });

            await remoteWatchController.SendConvoyConnectionParameters();

            var connectionParameters = await remoteWatchController.SendCategoryAndWaitForConnectionParams(0x16);  // Category id = 22 - route

            await remoteWatchController.SendConnectionSettingsBasedOnParams(connectionParameters, data.Length, 0x16);
        }
    }
}