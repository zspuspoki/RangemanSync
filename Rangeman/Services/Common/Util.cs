using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rangeman.Common
{
    internal static class Utils
    {
        public static string GetPrintableBytesArray(byte[] bytes, bool hideSensitiveData = true)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                if (!hideSensitiveData)
                {
                    sb.Append(b.ToString("X2") + ", ");
                }
                else
                {
                    sb.Append("??, ");
                }
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