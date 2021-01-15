using System;
using System.Linq;
using JetBrains.Annotations;
using Spotify;
using SpotifyLibV2.Helpers;

namespace SpotifyLibV2.Mercury
{
    public class MercuryResponse : IDisposable
    {
        public readonly string Uri;
        public readonly BytesArrayList Payload;
        public readonly int StatusCode;

        public MercuryResponse(
            [NotNull] Header header,
            [NotNull] BytesArrayList payload)
        {
            this.Uri = header.Uri;
            this.StatusCode = header.StatusCode;
            this.Payload = payload.CopyOfRange(1, payload.Count());
        }
        ~MercuryResponse()
        {
            Dispose();
        }
        public void Dispose()
        {
            Payload?.Dispose();
        }
    }
}