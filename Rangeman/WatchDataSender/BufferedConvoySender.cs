using Newtonsoft.Json.Converters;
using nexus.protocols.ble;
using System;
using System.Collections.Generic;

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

        public async void Send()
        {
            int currentConvoyDataCount = 0;
            int i = 0;
            currentConvoyDataCount = 0;

            List<byte> currentDataToSend = new List<byte>();
            List<byte> oneDataChunkWithCrc = new List<byte>();

            while (i < data.Length)
            {
                currentDataToSend.Add(0x05); // 0x05 is the type code of convoy data

                while (i++ < data.Length && currentConvoyDataCount++ < MaxNumberOfBytesToWriteConvoy)
                {
                    var dataToAdd = (byte)~(data[i]);
                    currentDataToSend.Add(dataToAdd);
                    oneDataChunkWithCrc.Add(dataToAdd);
                }

                if(i % 256 == 0)
                {
                    var crc16 = new Crc16(Crc16Mode.CcittKermit);
                    var crc = crc16.ComputeChecksumBytes(oneDataChunkWithCrc.ToArray());

                    foreach(var crcByte in crc)
                    {
                        currentDataToSend.Add(crcByte);
                    }

                    oneDataChunkWithCrc.Clear();
                }

                await gattServer.WriteCharacteristicValue(Guid.Parse(BLEConstants.CasioFeaturesServiceGuid),
                    Guid.Parse(BLEConstants.CasioConvoyCharacteristic), currentDataToSend.ToArray());

                currentDataToSend.Clear();
                currentConvoyDataCount = 0;
            }
        }
    }
}