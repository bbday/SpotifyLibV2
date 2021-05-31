using System;
using System.Linq;
using System.Text;
using Base62;
using Google.Protobuf;
using Spotify.Lib.Helpers;

namespace Spotify.Lib.Models.Ids
{
    public class ArtistId : StandardIdEquatable<ArtistId>
    {
        private static readonly Base62Test Base62Test
            = Base62Test.CreateInstanceWithInvertedCharacterSet();

        public ArtistId(string uri) : base(uri,
            uri.Split(':').Last(), AudioItemType.Artist)
        {
        }
        public static ArtistId FromGid(ByteString bts)
        {
            var k = Base62Test.Encode(bts.ToByteArray());
            var j = "spotify:artist:" + Encoding.UTF8.GetString(k);
            return new ArtistId(j);
        }
        public override string ToMercuryUri(string locale)
        {
            return $"hm://artist/v1/{Id}/desktop?format=json&catalogue=premium&locale={locale}&cat=1";
        }

        public override string ToHexId()
        {
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32) hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            return hex;
        }

        public static ArtistId FromHex(string hex)
        {
            var k = Base62Test.Encode(Utils.HexToBytes(hex));
            var j = "spotify:artist:" + Encoding.Default.GetString(k);
            return new ArtistId(j);
        }

        public string ToMercuryUriDetailed()
        {
            return
                $"hm://artist-identity-view/v2/profile/{Id}?fields=name,autobiography,biography,gallery,monthlyListeners,avatar&imgSize=large";
        }

        public string MercuryInsights()
        {
            return $"hm://creatorabout/v0/artist-insights/{Id}";
        }
    }
}