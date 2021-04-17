using System;
using System.Linq;
using Base62;
using MediaLibrary.Enums;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Ids
{
    public class ArtistId : StandardIdEquatable<ArtistId>
    {
        public ArtistId(string uri) : base(uri,
            uri.Split(':').Last(), AudioItemType.Artist, AudioServiceType.Spotify)
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

        public string ToMercuryUriDetailed() =>
            $"hm://artist-identity-view/v2/profile/{Id}?fields=name,autobiography,biography,gallery,monthlyListeners,avatar&imgSize=large";

        public string MercuryInsights() =>
            $"hm://creatorabout/v0/artist-insights/{Id}";
    }
}
