using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base62;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;

namespace SpotifyLibV2.Ids
{
    public class EpisodeId : IPlayableId
    {
        private readonly string _locale;
        private static Base62Test Base62Test =  Base62Test.CreateInstanceWithInvertedCharacterSet();
        public static EpisodeId FromHex(string hex)
        {
            //  return new ArtistId(Utils.bytesToHex(BASE62.decode(id.getBytes(), 16)));
            var k = Base62Test.Encode(Utils.HexToBytes(hex));
            var j = "spotify:show:" + Encoding.Default.GetString(k);
            return new EpisodeId(j);
        }
        public EpisodeId(string uri, string locale = "en")
        {
            _locale = locale;
            Type = AudioType.Episode;
            var regexMatch = uri.Split(':').Last();
            this.Id = regexMatch;
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

        public string ToMercuryUri() => $"hm://metadata/4/episode/{ToHexId()}?format=json&locale={_locale}";

        public AudioType Type { get; }

        public override bool Equals(object obj) => obj is EpisodeId trackId && trackId?.Uri == Uri;

        protected bool Equals(EpisodeId other)
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
