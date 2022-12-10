using nexus.protocols.ble;
using Rangeman;
using Rangeman.DataExtractors.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using RemoteWatchController = Rangeman.WatchDataReceiver.RemoteWatchController;
using System;
using Rangeman.WatchDataReceiver;
using Microsoft.Extensions.Logging;

namespace Rangeman.Services.WatchDataReceiver
{
    internal class LogPointMemoryExtractorService
    {
        public event EventHandler<DataReceiverProgressEventArgs> ProgressChanged;
        private RemoteWatchController remoteWatchController;
        private ILogger<LogPointMemoryExtractorService> logger;
        private readonly ILoggerFactory loggerFactory;

        public LogPointMemoryExtractorService(BlePeripheralConnectionRequest connection, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<LogPointMemoryExtractorService>();
            this.loggerFactory = loggerFactory;
            var gattServer = connection.GattServer;
            remoteWatchController = new RemoteWatchController(gattServer, loggerFactory);
        }

        public async Task<List<LogHeaderDataInfo>> GetHeaderDataAsync()
        {
            await remoteWatchController.SendInitializationCommandsToWatch();

            //REceive StartReadyToTransDataSequence

            var allDataReceived = new TaskCompletionSource<IDataExtractor>();
            var logAndPointMemoryHeaderParser = new LogAndPointMemoryHeaderParser(loggerFactory);

            var casioConvoyAndCasioDataRequestObserver = new CasioConvoyAndCasioDataRequestObserver(logAndPointMemoryHeaderParser,
                remoteWatchController, allDataReceived, loggerFactory);
            casioConvoyAndCasioDataRequestObserver.ProgressChanged += CasioConvoyAndCasioDataRequestObserver_ProgressChanged;

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            FireProgressChangedEvent("Sending download commands to watch.");
            await remoteWatchController.SendDownloadLogCommandsToWatch();

            FireProgressChangedEvent("Sending download log header commands to watch.");
            await remoteWatchController.SendDownloadHeaderCommandToWatch();

            //PreviousDataTransmitReplayer previousDataTransmitReplayer = new PreviousDataTransmitReplayer(casioConvoyAndCasioDataRequestObserver);
            //previousDataTransmitReplayer.Execute();

            var headerResultFromWatch = await allDataReceived.Task;

            casioConvoyAndCasioDataRequestObserver.ProgressChanged -= CasioConvoyAndCasioDataRequestObserver_ProgressChanged;

            if (headerResultFromWatch is LogAndPointMemoryHeaderParser dataExtractor)
            {
                var result = new List<LogHeaderDataInfo>();

                for (var i = 1; i <= 20; i++)
                {
                    var headerToAdd = dataExtractor.GetLogHeaderDataInfo(i);

                    if (headerToAdd != null)
                    {
                        headerToAdd.OrdinalNumber = i;
                        headerToAdd.LogAddress = dataExtractor.GetLogAddress(i);
                        headerToAdd.LogTotalLength = dataExtractor.GetLogTotalLength(i);
                        result.Add(headerToAdd);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }

                return null;
            }

            return null;
        }

        public async Task<List<LogData>> GetLogDataAsync(int headerDataSize, int headerDataCount, int logAddress, int logTotalLength)
        {
            logger.LogDebug("GetLogDataAsync -- Before SendInitializationCommandsToWatch");

            await remoteWatchController.SendInitializationCommandsToWatch();

            logger.LogDebug("GetLogDataAsync -- After SendInitializationCommandsToWatch");

            //REceive StartReadyToTransDataSequence

            var allDataReceived = new TaskCompletionSource<IDataExtractor>();

            logger.LogDebug("GetLogDataAsync -- Before creating observer");
            var casioConvoyAndCasioDataRequestObserver = new CasioConvoyAndCasioDataRequestObserver(new LogDataExtractor(headerDataSize, headerDataCount, loggerFactory),
                remoteWatchController, allDataReceived, loggerFactory);
            casioConvoyAndCasioDataRequestObserver.ProgressChanged += CasioConvoyAndCasioDataRequestObserver_ProgressChanged;
            logger.LogDebug("GetLogDataAsync -- After creating observer");

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            logger.LogDebug("GetLogDataAsync -- Before SendDownloadLogCommandsToWatch");
            FireProgressChangedEvent("Sending download commands to watch.");
            await remoteWatchController.SendDownloadLogCommandsToWatch();
            logger.LogDebug("GetLogDataAsync -- After SendDownloadLogCommandsToWatch");

            logger.LogDebug("GetLogDataAsync -- Before SendPointMemoryOrLogDownload");
            FireProgressChangedEvent("Sending point memory or log download command.");
            await remoteWatchController.SendPointMemoryOrLogDownload(logAddress, logTotalLength);
            logger.LogDebug("GetLogDataAsync -- After SendPointMemoryOrLogDownload");

            var logDataResultFromWatch = await allDataReceived.Task;
            casioConvoyAndCasioDataRequestObserver.ProgressChanged -= CasioConvoyAndCasioDataRequestObserver_ProgressChanged;

            if (logDataResultFromWatch is LogDataExtractor dataExtractor)
            {
                return dataExtractor.GetAllLogData();
            }

            return null;
        }

        private void FireProgressChangedEvent(string message)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new DataReceiverProgressEventArgs { Text = message });
            }
        }

        private void CasioConvoyAndCasioDataRequestObserver_ProgressChanged(object sender, DataRequestObserverProgressChangedEventArgs e)
        {
            FireProgressChangedEvent(e.Text);
        }

    }
}