using System;
using System.Linq;
using JetBrains.Annotations;
using Spotify;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Models.Response
{
    public readonly struct MercuryResponse : IDisposable
    {
        public readonly string Uri;
        public readonly BytesArrayList Payload;
        public readonly int StatusCode;

        public MercuryResponse(
            [NotNull] Header header,
            [NotNull] BytesArrayList payload)
        {
            Uri = header.Uri;
            StatusCode = header.StatusCode;
            Payload = payload.CopyOfRange(1, payload.Count());
        }

        public void Dispose()
        {
            Payload?.Dispose();
        }
    }
}