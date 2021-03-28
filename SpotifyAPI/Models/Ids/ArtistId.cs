using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base62;
using SpotifyLibrary.Enum;
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

        public override string ToMercuryUri(string locale) =>
            $"hm://artist/v1/{Id}/desktop?format=json&catalogue=premium&locale={locale}&cat=1";

        public override string ToHexId()
        {
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32)
            {
                hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            }
            return hex;
        }
        public static ArtistId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:artist:" + k;
            return new ArtistId(j);
        }

    }
}
