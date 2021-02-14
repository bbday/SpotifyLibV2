using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Base62;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;

namespace SpotifyLibV2.Ids
{
    public class ShowId : ISpotifyId
    {
        private readonly string _locale;
        public bool Equals(IAudioId other)
        {
            if (other is ShowId albumId)
            {
                return albumId.Uri == Uri;
            }
            return false;
        }
        public static ShowId FromHex(string hex)
        {
            var k = (Utils.HexToBytes(hex)).ToBase62(true);
            var j = "spotify:show:" + k;
            return new ShowId(j);
        }

        public ShowId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = AudioType.Show;
            var regexMatch = Regex.Match(uri, "spotify:show:(.{22})");
            if (regexMatch.Success)
            {
                this.Id = regexMatch.Groups[1].Value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(uri), "Not a Spotify show ID: " + uri);
            }
            this.Uri = uri;
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

        public string ToMercuryUri() => throw new NotImplementedException();

        public AudioType Type { get; }

        protected bool Equals(ShowId other)
        {
            return Uri == other.Uri
                   && Id == other.Id
                   && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }

        public AudioIdType IdType { get; }
    }
}
