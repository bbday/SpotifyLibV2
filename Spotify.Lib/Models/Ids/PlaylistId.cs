using System;
using System.Linq;
using System.Text;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Ids
{
    public class PlaylistId : StandardIdEquatable<PlaylistId>
    {
        private static readonly Base62Test Base62Test
            = Base62Test.CreateInstanceWithInvertedCharacterSet();

        public PlaylistId(string uri, bool radio = false) : base(uri, uri.Split(':').Last(), AudioItemType.Playlist)
        {
            IsRadio = radio;
        }

        public bool IsRadio { get; }

        public static PlaylistId FromHex(string hex)
        {
            var k = Base62Test.Encode(Utils.HexToBytes(hex));
            var j = "spotify:playlist:" + Encoding.UTF8.GetString(k);
            return new PlaylistId(j);
        }

        public override string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public override string ToHexId()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(ISpotifyId other)
        {
            if (other is PlaylistId plistid) return plistid.Id == Id;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PlaylistId) obj);
        }
    }
}