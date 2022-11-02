using nexus.protocols.ble;
using Rangeman;
using Rangeman.WatchDataReceiver;
using Rangeman.DataExtractors.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace employeeID
{
    internal class LogPointMemoryExtractorService
    {
        private RemoteWatchController remoteWatchController;

        public LogPointMemoryExtractorService(BlePeripheralConnectionRequest connection)
        {
            var gattServer = connection.GattServer;
            remoteWatchController = new RemoteWatchController(gattServer);

        }

        public async Task<List<LogHeaderDataInfo>> GetHeaderDataAsync()
        {
            await remoteWatchController.SendInitializationCommandsToWatch();

            //REceive StartReadyToTransDataSequence

            var allDataReceived = new TaskCompletionSource<IDataExtractor>();
            var logAndPointMemoryHeaderParser = new LogAndPointMemoryHeaderParser();

            var casioConvoyAndCasioDataRequestObserver = new CasioConvoyAndCasioDataRequestObserver(logAndPointMemoryHeaderParser, 
                remoteWatchController, allDataReceived);

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            await remoteWatchController.SendDownloadLogCommandsToWatch();

            await remoteWatchController.SendDownloadHeaderCommandToWatch();

            var headerResultFromWatch = await allDataReceived.Task;

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

                return result;
            }

            return null;
        }

        public async Task<List<LogData>> GetLogDataAsync(int logEntryOrdinal, int headerDataSize, int headerDataCount, int logAddress, int logTotalLength)
        {
            Debug.WriteLine("GetLogDataAsync -- Before SendInitializationCommandsToWatch");

            await remoteWatchController.SendInitializationCommandsToWatch();

            Debug.WriteLine("GetLogDataAsync -- After SendInitializationCommandsToWatch");

            //REceive StartReadyToTransDataSequence

            var allDataReceived = new TaskCompletionSource<IDataExtractor>();

            Debug.WriteLine("GetLogDataAsync -- Before creating observer");
            var casioConvoyAndCasioDataRequestObserver = new CasioConvoyAndCasioDataRequestObserver(new LogDataExtractor(headerDataSize, headerDataCount),
                remoteWatchController, allDataReceived);
            Debug.WriteLine("GetLogDataAsync -- After creating observer");

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            Debug.WriteLine("GetLogDataAsync -- Before SendDownloadLogCommandsToWatch");
            await remoteWatchController.SendDownloadLogCommandsToWatch();
            Debug.WriteLine("GetLogDataAsync -- After SendDownloadLogCommandsToWatch");

            Debug.WriteLine("GetLogDataAsync -- Before SendPointMemoryOrLogDownload");
            await remoteWatchController.SendPointMemoryOrLogDownload(logAddress, logTotalLength);
            Debug.WriteLine("GetLogDataAsync -- After SendPointMemoryOrLogDownload");

            var logDataResultFromWatch = await allDataReceived.Task;

            if (logDataResultFromWatch is LogDataExtractor dataExtractor)
            {
                return dataExtractor.GetAllLogData();
            }

            return null;
        }

    }
}