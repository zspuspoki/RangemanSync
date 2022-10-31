using nexus.protocols.ble;
using Rangeman;
using Rangeman.WatchDataReceiver;
using Rangeman.DataExtractors.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace employeeID
{
    internal class LogPointMemoryExtractorService
    {
        private readonly BlePeripheralConnectionRequest connection;
        private RemoteWatchController remoteWatchController;
        private LogAndPointMemoryHeaderParser logAndPointMemoryHeaderParser;
        private CasioConvoyAndCasioDataRequestObserver casioConvoyAndCasioDataRequestObserver;

        public event EventHandler<List<LogData>> AllLogDataReceived;

        public LogPointMemoryExtractorService(BlePeripheralConnectionRequest connection)
        {
            this.connection = connection;
            var gattServer = connection.GattServer;
            remoteWatchController = new RemoteWatchController(gattServer);

        }

        public async Task<List<LogHeaderDataInfo>> GetHeaderDataAsync()
        {
            await remoteWatchController.SendInitializationCommandsToWatch();

            //REceive StartReadyToTransDataSequence

            var allDataReceived = new TaskCompletionSource<IDataExtractor>();
            logAndPointMemoryHeaderParser = new LogAndPointMemoryHeaderParser();

            casioConvoyAndCasioDataRequestObserver = new CasioConvoyAndCasioDataRequestObserver(logAndPointMemoryHeaderParser, 
                remoteWatchController, allDataReceived);

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            await remoteWatchController.SendDownloadLogCommandsToWatch();

            var headerResultFromWatch = await allDataReceived.Task;

            if (headerResultFromWatch is LogAndPointMemoryHeaderParser dataExtractor)
            {
                var result = new List<LogHeaderDataInfo>();

                for (var i = 1; i <= 20; i++)
                {
                    var headerToAdd = dataExtractor.GetLogHeaderDataInfo(i);
                    result.Add(headerToAdd);
                }

                return result;
            }

            return null;
        }

        public async Task<List<LogData>> GetLogDataAsync(int logEntryOrdinal)
        {
            if (logAndPointMemoryHeaderParser == null)
            {
                throw new NotSupportedException("GetHeaderDataAsync should be called first before running this method");
            }

            var headerDataInfo = logAndPointMemoryHeaderParser.GetLogHeaderDataInfo(logEntryOrdinal); // it should be between 1 and 20

            casioConvoyAndCasioDataRequestObserver.SetDataExtractor(new LogDataExtractor(headerDataInfo));

            var allDataReceived = new TaskCompletionSource<IDataExtractor>();
            casioConvoyAndCasioDataRequestObserver.RestartDataReceiving(allDataReceived);

            var logAddress = logAndPointMemoryHeaderParser.GetLogAddress(logEntryOrdinal);
            var logTotalLength = logAndPointMemoryHeaderParser.GetLogTotalLength(logEntryOrdinal);

            await remoteWatchController.SendPointMemoryOrLogDownload(logAddress, logTotalLength);

            var logDataResultFromWatch = await allDataReceived.Task;

            if (logDataResultFromWatch is LogDataExtractor dataExtractor)
            {
                return dataExtractor.GetAllLogData();
            }

            return null;
        }

    }
}