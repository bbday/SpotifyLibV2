using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Ids
{
    public class UnknownId : ISpotifyId
    {
        public UnknownId(string uri)
        {
            Uri = uri;
            Type = SpotifyType.Unknown;
        }
        public string Uri { get; }
        public string Id { get; }
        public string ToHexId()
        {
            throw new NotImplementedException();
        }

        public string ToMercuryUri()
        {
            throw new NotImplementedException();
        }

        public SpotifyType Type { get; }
    }
}
