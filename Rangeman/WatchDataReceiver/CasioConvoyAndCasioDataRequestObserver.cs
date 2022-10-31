using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Rangeman.WatchDataReceiver;
using Rangeman.Common;
using System.Threading.Tasks;

namespace Rangeman
{
    internal class CasioConvoyAndCasioDataRequestObserver : IObserver<Tuple<Guid, byte[]>>
    {
        private const int SectorSize = 256;   //The current sector size is set to 256 bytes

        private List<byte[]> data = new List<byte[]>();
        private int currentSectorIndex = 0;
        private int currentDataIndexOnCurrentSector = 0;
        private int headerSize;

        private int digestedByteCount = 0; // every 256 bytes - we have a CRC code
        private bool dataReceivingIsAllowed = true;

        private static readonly object key = new object();
        private IDataExtractor dataExtractor;
        private readonly RemoteWatchController remoteWatchController;
        private TaskCompletionSource<IDataExtractor> taskCompletionSource;

        public event EventHandler<IDataExtractor> AllDataReceived;

        public CasioConvoyAndCasioDataRequestObserver(IDataExtractor dataExtractor, RemoteWatchController remoteWatchController, TaskCompletionSource<IDataExtractor> taskCompletionSource)
        {
            this.dataExtractor = dataExtractor;
            this.remoteWatchController = remoteWatchController;
            this.taskCompletionSource = taskCompletionSource;
        }

        public void OnCompleted()
        {
            Debug.WriteLine("-- Finished with CasioConvoyAndCasioDataRequestObserver");
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Tuple<Guid, byte[]> value)
        {
            lock (key)
            {
                if(!dataReceivingIsAllowed)
                {
                    Debug.WriteLine("OnNext - CasioConvoyAndCasioDataRequestObserver - data receiving is not allowed. Returning.");
                    return;
                }

                Debug.WriteLine($"OnNext - CasioConvoyAndCasioDataRequestObserver  Guid = {value.Item1}  value = {Utils.GetPrintableBytesArray(value.Item2)}");

                if (value.Item1 == Guid.Parse(BLEConstants.CasioConvoyCharacteristic))
                {
                    if (value.Item2[0] == 5)
                    {
                        var bytesToAdd = value.Item2.ToList();
                        bytesToAdd.RemoveAt(0);  //Remove type code

                        digestedByteCount += bytesToAdd.Count;

                        if(digestedByteCount >= 256)
                        {
                            digestedByteCount = 0;

                            bytesToAdd.RemoveAt(bytesToAdd.Count - 1); // Remove two bytes CRC code from the end
                            bytesToAdd.RemoveAt(bytesToAdd.Count - 1); // Remove two bytes CRC code from the end
                        }

                        var bytesArrayToAdd = bytesToAdd.ToArray();

                        for(var i=0;i<bytesToAdd.Count; i++)
                        {
                            bytesArrayToAdd[i] = (byte)(~bytesArrayToAdd[i]);
                        }

                        //Debug.WriteLine($"OnNext - CasioConvoyAndCasioDataRequestObserver data length = {data.Count}");
                        byte[] currentSectorBytes = data[currentSectorIndex];

                        //if (currentDataIndexOnCurrentSector + bytesArrayToAdd.Length > currentSectorBytes.Length - 1)
                        if(SectorSize - currentDataIndexOnCurrentSector < bytesArrayToAdd.Length)
                        {
                            currentSectorIndex++;
                            currentSectorBytes = data[currentSectorIndex];
                            currentDataIndexOnCurrentSector = 0;
                        }

                        Array.Copy(bytesArrayToAdd, 0, currentSectorBytes, currentDataIndexOnCurrentSector, bytesArrayToAdd.Length);
                        currentDataIndexOnCurrentSector += bytesArrayToAdd.Length;

                        var sectorOffsetFromStart = currentSectorIndex * SectorSize;
                        Debug.WriteLine($"OnNext - CasioConvoyAndCasioDataRequestObserver - Current sector offset : {sectorOffsetFromStart} Data index of sector: {currentDataIndexOnCurrentSector}");

                        if(headerSize == sectorOffsetFromStart + currentDataIndexOnCurrentSector)
                        {
                            Debug.WriteLine("OnNext - CasioConvoyAndCasioDataRequestObserver - all the header data was received successfully.");

                            EndCurrentTransmission();
                        }
                    }
                }
                else if (value.Item1 == Guid.Parse(BLEConstants.CasioDataRequestSPCharacteristic))
                {
                    var receivedBytes = value.Item2;
                    if (value.Item2.Length >= 9)
                    {
                        Debug.WriteLine("OnNext - CasioConvoyAndCasioDataRequestObserver - CasioDataRequestSPCharacteristic : Received an array where the length >= 9");
                        headerSize = ((receivedBytes[9] & 255) << 24) | (receivedBytes[6] & 255) | ((receivedBytes[7] & 255) << 8) | ((receivedBytes[8] & 255) << 16);
                        Debug.WriteLine($"OnNext - CasioConvoyAndCasioDataRequestObserver - CasioDataRequestSPCharacteristic: Header size: {headerSize}");

                        var numberofSectorToBeAdded = headerSize / SectorSize + 1;

                        Debug.WriteLine($"OnNext - CasioConvoyAndCasioDataRequestObserver - CasioDataRequestSPCharacteristic - No of sectors : {numberofSectorToBeAdded}");

                        for (var i = 0; i < numberofSectorToBeAdded; i++)
                        {
                            byte[] emptySector = new byte[SectorSize];
                            data.Add(emptySector);
                        }
                    }
                    else
                    {
                        if(receivedBytes.Length >0)
                        {
                            if (receivedBytes[0] == 7)
                            {
                                Debug.WriteLine("-- The watch needs confirmation to continue sending the data, so let's send it back.");
                                remoteWatchController.SendConfirmationToContinueTransmission();
                            }
                            else if ( receivedBytes.Length >=2 && receivedBytes[0] == 0x09 && receivedBytes[1] == 0x10)
                            {
                                //TODO : end current transmission because we hve all the data that is needed
                                EndCurrentTransmission();
                            }
                        }
                    }
                }
            }
        }

        private void EndCurrentTransmission()
        {
            dataReceivingIsAllowed = false;

            var allReceivedData = Utils.GetAllDataArray(data);
            dataExtractor.SetData(allReceivedData);

            if(taskCompletionSource != null)
            {
                taskCompletionSource.SetResult(dataExtractor);
            }

            if (AllDataReceived != null)
            {
                AllDataReceived(this, dataExtractor);
            }
        }

        public void RestartDataReceiving(TaskCompletionSource<IDataExtractor> taskCompletionSource)
        {
            this.taskCompletionSource = taskCompletionSource;

            RestartDataReceiving();
        }

        public void RestartDataReceiving()
        {
            ReinitData();
            dataReceivingIsAllowed = true;
        }

        public void SetDataExtractor(IDataExtractor dataExtractor)
        {
            this.dataExtractor = dataExtractor;
        }

        private void ReinitData()
        {
            data.Clear();
            currentSectorIndex = 0;
            currentDataIndexOnCurrentSector = 0;
            digestedByteCount = 0;
        }
    }
}