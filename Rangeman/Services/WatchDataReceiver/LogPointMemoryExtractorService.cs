﻿using nexus.protocols.ble;
using Rangeman;
using Rangeman.DataExtractors.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using RemoteWatchController = Rangeman.WatchDataReceiver.RemoteWatchController;
using Rangeman.WatchDataSender;
using System;
using Rangeman.WatchDataReceiver;

namespace employeeID
{
    internal class LogPointMemoryExtractorService
    {
        public event EventHandler<DataReceiverProgressEventArgs> ProgressChanged;
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
            casioConvoyAndCasioDataRequestObserver.ProgressChanged += CasioConvoyAndCasioDataRequestObserver_ProgressChanged;

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            FireProgressChangedEvent("Sending download commands to watch.");
            await remoteWatchController.SendDownloadLogCommandsToWatch();

            FireProgressChangedEvent("Sending download log header commands to watch.");
            await remoteWatchController.SendDownloadHeaderCommandToWatch();

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
            casioConvoyAndCasioDataRequestObserver.ProgressChanged += CasioConvoyAndCasioDataRequestObserver_ProgressChanged;
            Debug.WriteLine("GetLogDataAsync -- After creating observer");

            remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

            Debug.WriteLine("GetLogDataAsync -- Before SendDownloadLogCommandsToWatch");
            FireProgressChangedEvent("Sending download commands to watch.");
            await remoteWatchController.SendDownloadLogCommandsToWatch();
            Debug.WriteLine("GetLogDataAsync -- After SendDownloadLogCommandsToWatch");

            Debug.WriteLine("GetLogDataAsync -- Before SendPointMemoryOrLogDownload");
            FireProgressChangedEvent("Sending point memory or log download command.");
            await remoteWatchController.SendPointMemoryOrLogDownload(logAddress, logTotalLength);
            Debug.WriteLine("GetLogDataAsync -- After SendPointMemoryOrLogDownload");

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
            if(ProgressChanged != null)
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