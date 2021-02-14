using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Base62;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;

namespace SpotifyLibV2.Ids
{
    public class AlbumId : ISpotifyId
    {
        private readonly string _locale;

        public bool Equals(IAudioId other)
        {
            if (other is AlbumId albumId)
            {
                return albumId.Uri == Uri;
            }
            return false;
        }
        //000000001a31aba1a2c3056a05980c095e2712c6
        public static AlbumId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:album:" + k;
            return new AlbumId(j);
        }

        public AlbumId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = AudioType.Album;
            var regexMatch = Regex.Match(uri, "spotify:album:(.{22})");
            if (regexMatch.Success)
            {
                this.Id = regexMatch.Groups[1].Value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(uri), "Not a Spotify album ID: " + uri);
            }
            this.Uri = uri;
            IdType = AudioIdType.Spotify;
        }

        public string Uri { get; }
        public string Id { get; }
        public string ToHexId()
        {
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32)
            {
                hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            }
            return hex;
        }

        public string ToMercuryUri() => $"hm://album/v1/album-app/album/{Uri}/desktop?country=jp&catalogue=premium&locale={_locale}";

        public AudioType Type { get; }

        protected bool Equals(AlbumId other)
        {
            return Uri == other.Uri;
        }

        public override int GetHashCode()
        {
            return (Uri != null ? Uri.GetHashCode() : 0);
        }

        public AudioIdType IdType { get; }

    }
}
