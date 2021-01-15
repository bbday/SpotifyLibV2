using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibV2.Helpers.Extensions;

namespace SpotifyLibV2.Helpers
{
    public static class Utils
    {
        internal static string RandomHexString(int length)
        {
            var bytes = new byte[length / 2];
            (new Random()).NextBytes(bytes);
            return bytes.BytesToHex(0, bytes.Length, false, length);
        }
    }
}
