using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rangeman.Common
{
    internal static class Utils
    {
        public static string GetPrintableBytesArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2") + ", ");
            }
            sb.Append("}");

            return sb.ToString();
        }

        public static byte[] GetAllDataArray(List<byte[]> data)
        {
            var output = new byte[data.Sum(arr => arr.Length)];
            using (var stream = new MemoryStream(output))
                foreach (var bytes in data)
                    stream.Write(bytes, 0, bytes.Length);
            return output;
        }
    }
}