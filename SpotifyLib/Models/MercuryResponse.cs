using System.Collections.Generic;
using System.Linq;
using Spotify;

namespace SpotifyLib.Models
{
    public readonly struct MercuryResponse
    {
        public readonly string Uri;
        public readonly IEnumerable<byte[]> Payload;
        public readonly int StatusCode;
        public long Sequence { get; }
        public MercuryResponse(
            Header header, IReadOnlyCollection<byte[]> payload, long sequence)
        {
            Sequence = sequence;
            Uri = header.Uri;
            StatusCode = header.StatusCode;
            Payload = payload.Skip(1)
                .Take(payload.Count - 1);
        }

    }
}
