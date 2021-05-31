using System;
using System.Linq;
using System.Text;
using Base62;
using Google.Protobuf;
using Spotify.Lib.Helpers;

namespace Spotify.Lib.Models.Ids
{
    public class AlbumId : StandardIdEquatable<AlbumId>
    {
        private static readonly Base62Test Base62Test
            = Base62Test.CreateInstanceWithInvertedCharacterSet();

        public AlbumId(string uri) : base(uri, uri.Split(':').Last(),
            AudioItemType.Album)
        {
        }

        public static AlbumId FromGid(ByteString bts)
        {
            var k = Base62Test.Encode(bts.ToByteArray());
            var j = "spotify:album:" + Encoding.UTF8.GetString(k); 
            return new AlbumId(j);
        }
        public static AlbumId FromHex(string hex)
        {
            var k = Utils.HexToBytes(hex).ToBase62(true);
            var j = "spotify:album:" + k;
            return new AlbumId(j);
        }

        public override string ToMercuryUri(string locale)
        {
            return
                $"hm://album/v1/album-app/album/{Uri}/desktop?country={SpotifyClient.Instance.Country}&catalogue=premium&locale={locale}";
        }


        public override string ToHexId()
        {
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32) hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            return hex;
        }
    }
}