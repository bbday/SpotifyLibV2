﻿using System;
using System.Linq;

namespace Spotify.Lib.Models
{
    public readonly struct MercuryResponse : IDisposable
    {
        public readonly string Uri;
        public readonly BytesArrayList Payload;
        public readonly int StatusCode;

        public MercuryResponse(
            Header header, BytesArrayList payload)
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