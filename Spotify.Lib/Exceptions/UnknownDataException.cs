using System;
using System.Diagnostics;

namespace Spotify.Lib.Exceptions
{
    public class UnknownDataException : Exception
    {
        public UnknownDataException(byte[] data) : base($"Read unknown data, {data.Length} len")
        {
            Debug.WriteLine($"Read unknown data, {data.Length} len");
            Data = data;
        }

        public byte[] Data { get; }
    }
}