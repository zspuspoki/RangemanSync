using nexus.protocols.ble;
using Rangeman.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rangeman.WatchDataSender
{
    internal class BufferedConvoySender
    {
        private const int MaxNumberOfBytesToWriteConvoy = 107;
        private readonly IBleGattServerConnection gattServer;
        private readonly byte[] data;

        public BufferedConvoySender(IBleGattServerConnection gattServer, byte[] data)
        {
            this.gattServer = gattServer;
            this.data = data;
        }

        public async Task Send()
        {
            int i = 0;
            int currentConvoyDataCount = 0;

            List<byte> currentDataToSend = new List<byte>();
            List<byte> oneDataChunkWithCrc = new List<byte>();

            Debug.WriteLine($"--- BufferedConvoySender - data.Length = {data.Length}");

            while (i < data.Length)
            {
                currentDataToSend.Add(0x05); // 0x05 is the type code of convoy data

                Debug.WriteLine($"--- BufferedConvoySender - currentConvoyDataCount = {currentConvoyDataCount}");

                while (i < data.Length && currentConvoyDataCount++ < MaxNumberOfBytesToWriteConvoy)
                {
                    var dataToAdd = (byte)~(data[i++]);
                    currentDataToSend.Add(dataToAdd);
                    oneDataChunkWithCrc.Add(dataToAdd);

                    if (i % 256 == 0)
                        break;
                }

                Debug.WriteLine($"--- BufferedConvoySender - i after while loop = {i}");

                if(i % 256 == 0)
                {
                    var crc16 = new Crc16(Crc16Mode.CcittKermit);
                    var crc = crc16.ComputeChecksumBytes(oneDataChunkWithCrc.ToArray());

                    Debug.WriteLine($"--- BufferedConvoySender - crc code  : {Utils.GetPrintableBytesArray(crc)}");

                    foreach(var crcByte in crc)
                    {
                        currentDataToSend.Add(crcByte);
                    }

                    oneDataChunkWithCrc.Clear();
                }

                var currentByteArrayToSend = currentDataToSend.ToArray();

                Debug.WriteLine($"-- BufferedConvoySender - before sending data: {Utils.GetPrintableBytesArray(currentByteArrayToSend)}");

                await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                    Guid.Parse(BLEConstants.CasioConvoyCharacteristic), currentByteArrayToSend);

                await Task.Delay(5);

                currentDataToSend.Clear();
                currentConvoyDataCount = 0;
            }
        }
    }
}