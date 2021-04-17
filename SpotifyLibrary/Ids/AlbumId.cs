using System;
using System.Linq;
using Base62;
using MediaLibrary.Enums;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Ids
{
    public class AlbumId : StandardIdEquatable<AlbumId>
    {
        public AlbumId(string uri) : base(uri, uri.Split(':').Last(), AudioItemType.Album, AudioServiceType.Spotify)
        {
        }
        public static AlbumId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:album:" + k;
            return new AlbumId(j);
        }
        public override string ToMercuryUri(string locale) 
            => $"hm://album/v1/album-app/album/{Uri}/desktop?country={SpotifyLibrary.Country}&catalogue=premium&locale={locale}";


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
    }
}
