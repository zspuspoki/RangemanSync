using Microsoft.Extensions.Logging;
using nexus.protocols.ble;
using Rangeman.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rangeman.WatchDataReceiver
{
    internal class RemoteWatchController
    {
        private const int CommandDelay = 20;
        private readonly IBleGattServerConnection gattServer;
        private readonly ILoggerFactory loggerFactory;
        private ILogger<RemoteWatchController> logger;

        public RemoteWatchController(IBleGattServerConnection gattServer, ILoggerFactory loggerFactory)
        {
            this.gattServer = gattServer;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<RemoteWatchController>();
        }

        public void SendMessageToDRSP(byte[] data)
        {
            // 00, 0F, 00, 10, 00, 00, 00, 20, 00, 00,
            var arrayToSend = new byte[] { 00, 0x0F, 00, 00, 00, 00, 00, 00, 00, 00 };
            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), data);
        }

        public void SendConfirmationToContinueTransmission()
        {
            var arrayToSend = new byte[] { 0x07, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), arrayToSend);
        }

        public void AskWatchToEndTransmission(byte categoryId)
        {
            var arrayToSend = new byte[] { 0x03, categoryId, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
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
            logger.LogDebug("-- Before  WriteCharacteristicValue 1");
            //TODO : Move it to else if (value.Item1 == Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic)) ?
            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x09, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            logger.LogDebug("-- After  WriteCharacteristicValue 1");

            gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x04, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            logger.LogDebug("-- After  WriteCharacteristicValue 2");
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

            await Task.Delay(CommandDelay);

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x01, 0x18, 0x00, 0x18, 0x00, 0x00, 0x00, 0x58, 0x02 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x02, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x02, 0xF0, 0x00, 0x10, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF });

            await Task.Delay(CommandDelay);
        }

        public async Task SendDownloadHeaderCommandToWatch()
        {
            //8192 - sector size 0x2000
            //0x0F - category ID : header
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, 0x0F, 0x00, 0x10, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00 });

            await Task.Delay(CommandDelay);
        }

        public async Task SendInitializationCommandsToWatch()
        {
            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioAllFeaturesCharacteristic), new CharChangedObserver(loggerFactory));

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioReadRequestForAllFeaturesCharacteristic), new byte[] { 0x11 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x00, 0x00, 0x00 });
        }

        public async Task StartCloseSequence()
        {
            logger.LogDebug("StartCloseSequence() -- Start");

            await Task.Delay(CommandDelay);

            await StartCloseSequence_Sub1();

            await Task.Delay(CommandDelay);

            await StartCloseSequence_Sub2();

            await Task.Delay(CommandDelay);

            logger.LogDebug("StartCloseSequence() -- Before writing 0x04, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00");

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x04, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            await Task.Delay(CommandDelay);

            logger.LogDebug("StartCloseSequence() -- Before writing 0x04, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00");

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x04, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                      Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                      Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x00, 0x00 });

            await Task.Delay(CommandDelay);

            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x00, 0x00 });

            await Task.Delay(CommandDelay);

        }

        private async Task StartCloseSequence_Sub1()
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                (data) =>
                {
                    logger.LogDebug($"--- StartCloseSequence_Sub1 - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");
                   
                    if(data.SequenceEqual(new byte[] { 0x04, 0x00, 0x18, 0x00, 0x18, 0x00, 0x00, 0x00, 0x58, 0x02 }))
                    {
                        taskCompletionSource.SetResult(data);
                    }
                });

            await Task.Delay(CommandDelay);

            logger.LogDebug("StartCloseSequence_Sub1() -- Before writing 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00");

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            //await taskCompletionSource.Task;
        }

        private async Task StartCloseSequence_Sub2()
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                (data) =>
                {
                    logger.LogDebug($"--- StartCloseSequence_Sub2 - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");

                    if (data.SequenceEqual(new byte[] { 0x04, 0x00, 0x18, 0x00, 0x18, 0x00, 0x00, 0x00, 0x58, 0x02 }))
                    {
                        taskCompletionSource.SetResult(data);
                    }
                });

            await Task.Delay(CommandDelay);

            logger.LogDebug("StartCloseSequence_Sub2() -- Before writing 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00");

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x01, 0x48, 0x00, 0x50, 0x00, 0x04, 0x00, 0x58, 0x02 });

            await taskCompletionSource.Task;
        }
    }
}