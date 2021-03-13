using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Base62;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;

namespace SpotifyLibV2.Ids
{
    public class TrackId : IPlayableId
    {
        private static readonly Base62Test Base62Test 
            = Base62Test.CreateInstanceWithInvertedCharacterSet();

        public bool Equals(IAudioId other)
        {
            if (other is TrackId genid)
            {
                return genid.Uri == Uri;
            }
            return false;
        }
        private readonly string _locale;
        public TrackId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = AudioType.Track;
            var regexMatch = uri.Split(':').Last();
            this.Id = regexMatch;
            this.Uri = uri;
            IdType = AudioIdType.Spotify;
        }
        public static TrackId FromHex(string hex)
        {
            //  return new ArtistId(Utils.bytesToHex(BASE62.decode(id.getBytes(), 16)));
            var k = Base62Test.Encode(Utils.HexToBytes(hex));
            var j = "spotify:track:" + Encoding.Default.GetString(k);
            return new TrackId(j);
        }
        public string Uri { get; set; }
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

        public string ToMercuryUri() => $"hm://metadata/4/track/{ToHexId()}?locale={_locale}";
        public AudioType Type { get; set; }

        public override bool Equals(object obj) => obj is TrackId trackId && trackId.Uri == Uri;

        public bool Equals(TrackId other)
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

        public override string ToString()
        {
            return Uri;
        }
    }
}

