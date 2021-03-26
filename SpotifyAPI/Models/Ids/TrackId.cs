using System;
using System.Linq;
using Base62;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
    public class TrackId : StandardIdEquatable<TrackId>
    {
        public TrackId(string uri) :
            base(uri, uri.Split(':').Last(), AudioType.Track, AudioService.Spotify) { }

        public override string ToMercuryUri(string locale)
        {
            //TODO
            throw new System.NotImplementedException();
        }

        public override string ToHexId()
        { //TODO
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32)
            {
                hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            }
            return hex;
        }
    }
}
