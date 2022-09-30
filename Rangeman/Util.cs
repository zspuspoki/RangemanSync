using System;
using System.Text;

namespace Rangeman
{
    internal static class Util
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
    }
}