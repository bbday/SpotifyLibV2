using System;
using System.Linq;
using SpotifyLib.Models;

namespace SpotifyLib
{
    public class MercuryException : Exception
    {
        public MercuryException(MercuryResponse? response)
        {
            Response = response;
        }
        public MercuryResponse? Response { get; }
        public uint StatusCode => (uint)(Response?.StatusCode ?? 0);

        public string PayloadAsString =>
            System.Text.Encoding.UTF8.GetString(Response?.Payload.SelectMany(z => z).ToArray() ?? Array.Empty<byte>());
    }
}

