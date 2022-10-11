using System;

namespace Rangeman.WatchDataSender
{
    internal class ConnectionParameters
    {
        private readonly byte[] data;

        public int MtuSize { get; set; }
        public long DataSizeOf1Sector { get; set; }
        public long Offset1Sector { get; set; }
        public sbyte AreaNumber { get; set; }

        public ConnectionParameters(byte[] data)
        {
            this.data = data;

            GetMtuSize();
            GetDataSizeOf1Sector();
            GetOffset1Sector();
            GetAreaNumber();
        }

        private void GetAreaNumber()
        {
            int i = 0;
            byte kindOfData = data[0];

            if (kindOfData == 2)
            {
                i = 10;
            }
            else if (kindOfData != 6)
            {
                AreaNumber = (sbyte)-1;
            }
            else
            {
                i = 11;
            }

            if (data == null || data.Length <= i)
            {
                AreaNumber = (sbyte)-1;
            }

            AreaNumber = (sbyte)data[i];
        }

        private void GetOffset1Sector()
        {
            int i = 0;
            byte kindOfData = data[0];

            if (kindOfData == 2)
            {
                i = 9;
            }
            else if (kindOfData != 6)
            {
                Offset1Sector = -1L;
            }
            else
            {
                i = 10;
            }

            if (data == null || data.Length <= i)
            {
                Offset1Sector = 0L;
            }

            Offset1Sector = (data[i - 3] & 255) | ((data[i] & 255) << 24) | ((data[i - 1] & 255) << 16) | ((data[i - 2] & 255) << 8);
        }

        private void GetDataSizeOf1Sector()
        {
            byte kindOfData = data[0];

            int i = 6;

            if (kindOfData == 2)
            {
                i = 5;
            }

            if (data != null && data.Length > i)
            {
                DataSizeOf1Sector = ((data[i] & 255) << 24) | ((data[i - 1] & 255) << 16) | ((data[i - 2] & 255) << 8) | (data[i - 3] & 255);
            }

            DataSizeOf1Sector = -1L;
        }

        private void GetMtuSize()
        {
            byte kindOfData = data[0];

            if (kindOfData == 2)
            {
                if (data != null && data.Length > 1)
                {
                    MtuSize = data[1] & 255;
                }

                MtuSize = -1;
            }
            else if (kindOfData == 6 && data.Length > 2)
            {
                MtuSize = (data[1] & 255) | ((data[2] & 255) << 8);
            }
            else
            {
                MtuSize = -1;
            }
        }
    }
}