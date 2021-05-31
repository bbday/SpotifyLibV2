using System;

namespace Spotify.Lib.Helpers
{
    public static class Utils
    {
        internal static string RandomHexString(int length)
        {
            var bytes = new byte[length / 2];
            new Random().NextBytes(bytes);
            return bytes.BytesToHex(0, bytes.Length, false, length);
        }

        public static byte[] HexToBytes(string str)
        {
            var len = str.Length;
            var data = new byte[len / 2];
            for (var i = 0; i < len; i += 2)
                data[i / 2] = (byte) ((Convert.ToInt32(str[i].ToString(),
                    16) << 4) + Convert.ToInt32(str[i + 1].ToString(), 16));
            return data;
        }
    }
}