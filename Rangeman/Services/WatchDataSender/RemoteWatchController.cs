using Microsoft.Extensions.Logging;
using nexus.protocols.ble;
using Rangeman.Common;
using Rangeman.Services.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rangeman.WatchDataSender
{
    internal class RemoteWatchController
    {
        private const int CommandDelay = 20;
        private readonly IBleGattServerConnection gattServer;
        private ILogger<RemoteWatchController> logger;

        public RemoteWatchController(IBleGattServerConnection gattServer, ILoggerFactory loggerFactory)
        {
            this.gattServer = gattServer;
            this.logger = loggerFactory.CreateLogger<RemoteWatchController>();
        }

        public async Task SendInitCommandsAndWaitForCCCData(byte[] convoyData)
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
            (data) =>
            {
                logger.LogDebug($"--- SendInitCommandsAndWaitForCCCData - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");

                if (data.SequenceEqual(convoyData))
                {
                    logger.LogDebug($"--- SendInitCommandsAndWaitForCCCData - NotifyCharacteristicValue. data equals to convoy data : {Utils.GetPrintableBytesArray(convoyData)}");
                    taskCompletionSource.SetResult(data);
                }
            }, 
            ()=> taskCompletionSource.TrySetResult(new byte[] { }));

            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - Before writing CasioDataRequestSPCharacteristic");
            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                      Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                                      Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });

            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - before executing first delay");
            await Task.Delay(CommandDelay);
            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - after executing first delay");

            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - Before writing CasioConvoyCharacteristic");
            await gattServer.WriteDescriptorValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                                  Guid.Parse(BLEConstants.CCCDescriptor), new byte[] { 0x01, 0x00 });

            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - before executing second delay");
            await Task.Delay(CommandDelay);
            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - after executing second delay");

            logger.LogDebug("--- SendInitCommandsAndWaitForCCCData - Before writing CasioDataRequestSPCharacteristic 2.");
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, 0x12, 0x00, 0x00 }); // Category = 18 - connection setup

            await Task.Delay(CommandDelay);

            await taskCompletionSource.Task;
        }

        public async Task SendConvoyConnectionParameters()
        {
            logger.LogDebug("--- SendConvoyConnectionParameters - Before writing CasioConvoyCharacteristic");
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x01, 0x18, 0x00, 0x18, 0x00, 0x00, 0x00, 0x58, 0x02 });

            await Task.Delay(CommandDelay);
        }

        public async Task<ConnectionParameters> SendCategoryAndWaitForConnectionParams(byte categoryId)
        {
            var taskCompletionSource = new TaskCompletionSource<ConnectionParameters>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
            (data) =>
            {
                logger.LogDebug($"--- SendCategoryAndWaitForConnectionParams - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");
                var kindData = data[0];

                if (kindData == 2 || kindData == 6)
                {
                    var connectionParameters = new ConnectionParameters(data);

                    taskCompletionSource.SetResult(connectionParameters);
                }
            },
            () => taskCompletionSource.TrySetResult(new ConnectionParameters(new byte[] { })));

            logger.LogDebug("--- SendCategoryAndWaitForConnectionParams - Before writing CasioDataRequestSPCharacteristic");
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                                  Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x00, categoryId, 0x00, 0x00 });

            await Task.Delay(CommandDelay);

            var result = await taskCompletionSource.Task;

            logger.LogDebug("--- SendCategoryAndWaitForConnectionParams - after awaiting Task");
            return result;
        }

        public async Task SendConnectionSettingsBasedOnParams(ConnectionParameters parameters, int totalDataLength, byte categoryId)
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
            (data) =>
            {
                logger.LogDebug($"--- SendConnectionSettingsBasedOnParams - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");
                if (data[0] == 0 && data[1] == categoryId)
                { 
                    taskCompletionSource.SetResult(data);
                }
            },
            () => taskCompletionSource.TrySetResult(new byte[] { }));


            long currentParameterDataSizeOf1Sector = parameters.DataSizeOf1Sector * parameters.OffsetSector;
            long j3 = totalDataLength - currentParameterDataSizeOf1Sector;

            logger.LogDebug($"--- SendConnectionSettingsBasedOnParams - currentParameterDataSizeOf1Sector : {currentParameterDataSizeOf1Sector}");
            logger.LogDebug($"--- SendConnectionSettingsBasedOnParams - j3 : {j3}");

            if (j3>0)
            {
                //Create convoy data: totalDataLength, j3, acceptor1, acceptor2, timeoutMinute
                //Acceptor1: 250
                //Acceptor2: 245
                
                var convoyData = CreateConvoyData(totalDataLength, j3, 250, 245,  0);

                logger.LogDebug($"--- SendConnectionSettingsBasedOnParams - after CreateConvoyData. convoyData = {convoyData}");

                await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                        Guid.Parse(BLEConstants.CasioConvoyCharacteristic), convoyData);

                await Task.Delay(CommandDelay);
            }

            logger.LogDebug($"--- SendConnectionSettingsBasedOnParams - before awaiting task");
            await taskCompletionSource.Task;
            logger.LogDebug($"--- SendConnectionSettingsBasedOnParams - after awaiting task");
        }

        public async Task CloseCurrentCategoryAndWaitForResponse(byte categoryId)
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic),
                (data) =>
                {
                    logger.LogDebug($"--- CloseCurrentCategoryAndWaitForResponse - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");
                    //if (data[0] == 4)
                    //{
                    //    if (data[1] == categoryId)
                    //    {
                    //        taskCompletionSource.SetResult(data);
                    //    }
                    //}
                    if(data.SequenceEqual(new byte[] { 09, categoryId , 00, 00, 00, 00, 00 }))
                    {
                        logger.LogDebug("--- CloseCurrentCategoryAndWaitForResponse - Sequence is equals");
                        taskCompletionSource.SetResult(data);
                    }
                },
            () => taskCompletionSource.TrySetResult(new byte[] { }));

            logger.LogDebug($"--- CloseCurrentCategoryAndWaitForResponse - Before awaiting task");
            await taskCompletionSource.Task;
            logger.LogDebug($"--- CloseCurrentCategoryAndWaitForResponse - After awaiting task");

            logger.LogDebug($"--- CloseCurrentCategoryAndWaitForResponse - Before sending values to characteristic");
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x09, categoryId, 0x00, 0x00, 0x00 });

            await Task.Delay(CommandDelay);
        }

        public async Task WriteFinalClosingData()
        {
            var taskCompletionSource = new TaskCompletionSource<byte[]>();

            gattServer.NotifyCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid), Guid.Parse(BLEConstants.CasioConvoyCharacteristic),
                (data) =>
                {
                    logger.LogDebug($"--- WriteFinalClosingData - NotifyCharacteristicValue. Received data bytes : {Utils.GetPrintableBytesArray(data)}");
                    if (data[0] == 0x04 && data[1] == 0x00 && data[2] == 0x18)
                    {
                        taskCompletionSource.SetResult(data);
                    }
                },
            () => taskCompletionSource.TrySetResult(new byte[] { }));

            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic), new byte[] { 0x04, 0x12, 0x00, 0x00, 0x00 });  // category 18

            await Task.Delay(CommandDelay);

            logger.LogDebug($"--- WriteFinalClosingData - Before awaiting task");
            await taskCompletionSource.Task;
            logger.LogDebug($"--- WriteFinalClosingData - After awaiting task");
        }

        public async Task WriteFinalClosingData2()
        {
            logger.LogDebug($"--- WriteFinalClosingData2 - Start");
            await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                Guid.Parse(BLEConstants.CasioConvoyCharacteristic), new byte[] { 0x04, 0x01, 0x48, 0x00, 0x50, 0x00, 0x04, 0x00, 0x58, 0x02 });

            await Task.Delay(CommandDelay);
            logger.LogDebug("--- WriteFinalClosingData2 - End");
        }

        private static byte[] CreateConvoyData(long j, long j2, int i, int i2, int i3)
        {
            return new byte[]{1, (byte) (j & 255), (byte) ((j >>> 8) & 255), (byte) ((j >>> 16) & 255), (byte) ((j >>> 24) & 255), (byte) (j2 & 255), (byte) ((j2 >>> 8) & 255), (byte) ((j2 >>> 16) & 255), (byte) ((j2 >>> 24) & 255), (byte) (i & 255), (byte) (i2 & 255), (byte) (i3 & 255)};
        }
    }
}