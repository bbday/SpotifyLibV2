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
    public class PlaylistId : StandardIdEquatable<PlaylistId>
    {
        public PlaylistId(string uri) : base(uri, uri.Split(':').Last(), AudioType.Playlist, AudioService.Spotify)
        {
        }
        public static PlaylistId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:album:" + k;
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
    }
}
