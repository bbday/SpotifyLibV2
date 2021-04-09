using System;
using System.Linq;
using System.Text;
using Base62;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Models.Ids
{
    public class PlaylistId : StandardIdEquatable<PlaylistId>
    {
        private static readonly Base62Test Base62Test
            = Base62Test.CreateInstanceWithInvertedCharacterSet();
        public PlaylistId(string uri, bool radio = false) : base(uri, uri.Split(':').Last(), AudioType.Playlist, AudioService.Spotify)
        {
            IsRadio = radio;
        }
        public bool IsRadio { get; }
        public static PlaylistId FromHex(string hex)
        {
            var k = Base62Test.Encode(Utils.HexToBytes(hex));
            var j = "spotify:playlist:" + Encoding.Default.GetString(k);
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

        public override bool Equals(IAudioId other)
        {
            if (other is PlaylistId plistid)
            {
                return plistid.Id == Id;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlaylistId)obj);
        }
    }
}
