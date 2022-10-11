using nexus.protocols.ble;
using System;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Rangeman.WatchDataReceiver
{
    internal class RemoteWatchController
    {
        private readonly IBleGattServerConnection gattServer;

        public RemoteWatchController(IBleGattServerConnection gattServer)
        {
            this.gattServer = gattServer;
        }

        public void SendConfirmationToContinueTransmission()
        {
            var arrayToSend = new byte[] { 0x07, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), arrayToSend);
        }

        /// <summary>
        /// Send command to the watch to download point memory or log entry
        /// </summary>
        /// <param name="address">result of GetLogAddress(i) or GetPointMemoryAddress</param>
        /// <param name="length">result of GetLogTotalLength(i) or GetPointMemoryTotalLength</param>
        public async Task SendPointMemoryOrLogDownload(int address, int length)
        {
            var b = (byte)0;
            var b2 = (byte)16;
            byte[] arrayToSend = { b, b2,
                (byte)(address & 255), (byte)((address >>> 8) & 255),
                (byte)((address >>> 16) & 255), (byte)((address >>> 24) & 255),
                (byte)(length & 255), (byte)((length >>> 8) & 255),
                (byte)((length >>> 16) & 255), (byte)((length >>> 24) & 255) };

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), arrayToSend);
        }

        public void SendHeaderClosingCommandsToWatch()
        {
            Debug.WriteLine("-- Before  WriteCharacteristicValue 1");
            //TODO : Move it to else if (value.Item1 == Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic)) ?
            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x09, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            Debug.WriteLine("-- After  WriteCharacteristicValue 1");

            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x04, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            Debug.WriteLine("-- After  WriteCharacteristicValue 2");
        }

        public void SubscribeToCharacteristicChanges(CasioConvoyAndCasioDataRequestObserver casioConvoyAndCasioDataRequestObserver)
        {
            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                casioConvoyAndCasioDataRequestObserver);

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                casioConvoyAndCasioDataRequestObserver);
        }

        public async Task SendDownloadLogCommandsToWatch()
        {
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });


            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x01, 0x18, 0x00, 0x18, 0x00, 0x00, 0x00, 0x58, 0x02 });


            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x02, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });


            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x02, 0xF0, 0x00, 0x10, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF });

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, 0x0F, 0x00, 0x10, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00 });
        }

        public async Task SendInitializationCommandsToWatch()
        {
            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioAllFeaturesCharacteristic), new CharChangedObserver());

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioReadRequestForAllFeaturesCharacteristic), new byte[] { 0x11 });

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });


            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });


            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });


            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x00, 0x00, 0x00 });
        }

    }
}