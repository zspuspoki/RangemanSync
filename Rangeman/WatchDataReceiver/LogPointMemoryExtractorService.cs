using nexus.protocols.ble;
using Rangeman;
using Rangeman.WatchDataReceiver;
using Rangeman.DataExtractors.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace employeeID
{
    internal class LogPointMemoryExtractorService
    {
        private readonly BlePeripheralConnectionRequest connection;
        public event EventHandler<List<LogData>> AllLogDataReceived;

        public LogPointMemoryExtractorService(BlePeripheralConnectionRequest connection)
        {
            this.connection = connection;

        }

        public async void StartDownloadLogAndPointMemoryData()
        {
            try
            {
                Debug.WriteLine("Inside StartDownloadLogAndPointMemoryData");

                var gattServer = connection.GattServer;
                var remoteWatchController = new RemoteWatchController(gattServer);

                await remoteWatchController.SendInitializationCommandsToWatch();

                //REceive StartReadyToTransDataSequence

                var casioConvoyAndCasioDataRequestObserver = new CasioConvoyAndCasioDataRequestObserver(new LogAndPointMemoryHeaderParser(), remoteWatchController);

                remoteWatchController.SubscribeToCharacteristicChanges(casioConvoyAndCasioDataRequestObserver);

                casioConvoyAndCasioDataRequestObserver.AllDataReceived += async (s, d) =>
                {
                    var sender = s as CasioConvoyAndCasioDataRequestObserver;
                    if (sender != null)
                    {
                        switch (d)
                        {
                            case LogAndPointMemoryHeaderParser logAndPointMemoryHeaderParser:
                                remoteWatchController.SendHeaderClosingCommandsToWatch();
                                var firstHeaderInfo = logAndPointMemoryHeaderParser.GetLogHeaderDataInfo(4); // it should be between 1 and 20
                                Debug.WriteLine($"----Header data. (1st) dataCount = {firstHeaderInfo.DataCount} dataSize = {firstHeaderInfo.DataSize}");

                                sender.SetDataExtractor(new LogDataExtractor(firstHeaderInfo));
                                sender.RestartDataReceiving();

                                var logAddress = logAndPointMemoryHeaderParser.GetLogAddress(4);
                                var logTotalLength = logAndPointMemoryHeaderParser.GetLogTotalLength(4);

                                await remoteWatchController.SendPointMemoryOrLogDownload(logAddress, logTotalLength);
                                break;

                            case LogDataExtractor logDataExtractor:
                                Debug.WriteLine("-- Finished in event handler - logDataExtractor");
                                //logDataExtractor.GetLogData(0);
                                
                                var allLogData = logDataExtractor.GetAllLogData();
                                if(AllLogDataReceived != null)
                                {
                                    AllLogDataReceived(this, allLogData);
                                }
                                break;
                        }
                    }
                };

                await remoteWatchController.SendDownloadLogCommandsToWatch();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                //await connection.GattServer.Disconnect();
            }
        }

    }
}