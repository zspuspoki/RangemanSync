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

            var categoriesToSend = new CategoryToSend[] { 
                new CategoryToSend(0x16, data), 
                new CategoryToSend(0x15, header) };

            foreach (var category in categoriesToSend)
            {
                var connectionParameters = await remoteWatchController.SendCategoryAndWaitForConnectionParams(category.CategoryId);  // Category id = 22 - route

                await remoteWatchController.SendConnectionSettingsBasedOnParams(connectionParameters, data.Length, category.CategoryId);

                BufferedConvoySender bufferedConvoySender = new BufferedConvoySender(this.connection.GattServer, category.Data);
                bufferedConvoySender.Send();

                await remoteWatchController.CloseCurrentCategoryAndWaitForResponse(category.CategoryId);
            }

            await remoteWatchController.WriteFinalClosingData();

            await remoteWatchController.WriteFinalClosingData2();
        }
    }
}