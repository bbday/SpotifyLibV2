using System;
using System.Linq;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace SpotifyLib.Helpers
{
    public static class ByteExtensions
    {
        private static readonly char[] HexArray = "0123456789ABCDEF".ToCharArray();

        internal static byte[] ToByteArray(this int i)
        {
            var bytes = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian) return bytes.Reverse().ToArray();

            return bytes;
        }
        internal static byte[] ToByteArray(this BigInteger i)
        {
            var array = i.ToByteArray();
            if (array[0] == 0) array = Arrays.CopyOfRange(array, 1, array.Length);
            return array;
        }
        public static string BytesToHex(this byte[] bytes)
        {
            return BytesToHex(bytes, 0, bytes.Length, true, -1);
        }
        internal static string BytesToHex(this byte[] bytes, int offset, int length, bool trim, int minLength)
        {
            if (bytes == null) return "";

            var newOffset = 0;
            var trimming = trim;
            var hexChars = new char[length * 2];
            for (var j = offset; j < length; j++)
            {
                var v = bytes[j] & 0xFF;
                if (trimming)
                {
                    if (v == 0)
                    {
                        newOffset = j + 1;

                        if (minLength != -1 && length - newOffset == minLength)
                            trimming = false;

                        continue;
                    }

                    trimming = false;
                }

                hexChars[j * 2] = HexArray[(int)((uint)v >> 4)];
                hexChars[j * 2 + 1] = HexArray[v & 0x0F];
            }

            return new string(hexChars, newOffset * 2, hexChars.Length - newOffset * 2);
        }
    }
}
