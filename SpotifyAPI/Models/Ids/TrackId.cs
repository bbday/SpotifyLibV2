using System;
using System.Linq;
using System.Text;
using Base62;
using MusicLibrary.Enum;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Models.Ids
{
    public class TrackId : StandardIdEquatable<TrackId>
    {
        private static readonly Base62Test Base62Test
            = Base62Test.CreateInstanceWithInvertedCharacterSet();

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

        public static TrackId FromHex(string hex)
        {
            //  return new ArtistId(Utils.bytesToHex(BASE62.decode(id.getBytes(), 16)));
            var k = Base62Test.Encode(Utils.HexToBytes(hex));
            var j = "spotify:track:" + Encoding.Default.GetString(k);
            return new TrackId(j);
        }

        public override string ToString() => Uri;
    }
}
