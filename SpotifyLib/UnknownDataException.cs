﻿using System;
using System.Diagnostics;

namespace SpotifyLib
{
    public class UnknownDataException : Exception
    {
        internal UnknownDataException(string cmd, byte[] data)
        {
            Debug.WriteLine(cmd);
            Data = data;
        }
        internal UnknownDataException(byte[] data) : base($"Read unknown data, {data.Length} len")
        {
            Debug.WriteLine($"Read unknown data, {data.Length} len");
            Data = data;
        }

        public byte[] Data { get; }
    }
}
