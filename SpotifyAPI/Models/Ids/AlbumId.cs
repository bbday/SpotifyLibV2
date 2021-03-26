using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base62;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
    public class AlbumId : StandardIdEquatable<AlbumId>
    {
        public AlbumId(string uri) : base(uri, uri.Split(':').Last(), AudioType.Album, AudioService.Spotify)
        {
        }
        public static AlbumId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:album:" + k;
            return new AlbumId(j);
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
