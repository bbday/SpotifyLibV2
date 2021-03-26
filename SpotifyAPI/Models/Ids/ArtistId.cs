using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base62;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
    public class ArtistId : StandardIdEquatable<ArtistId>
    {
        public ArtistId(string uri) : base(uri,
            uri.Split(':').Last(), AudioType.Artist, AudioService.Spotify)
        {
        }

        public override string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public override string ToHexId()
        {
            throw new NotImplementedException();
        }
        public static ArtistId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:artist:" + k;
            return new ArtistId(j);
        }

    }
}
