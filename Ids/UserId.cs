using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spotify.Social;
using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Ids
{
    public class UserId : ISpotifyId
    {
        public bool Equals(IAudioId other)
        {
            if (other is UserId genid)
            {
                return genid.Uri == Uri;
            }
            return false;
        }
        public UserId(string uri)
        {
            Type = AudioType.Profile;
            IdType = AudioIdType.Spotify;
            var regexMatch = uri.Split(':').Last();
            this.Id = regexMatch;
            this.Uri = uri;
        }

        public AudioIdType IdType { get; }
        public string Uri { get; }
        public string Id { get; }
        public string ToHexId()
        {
            throw new NotImplementedException();
        }

        public string ToMercuryUri()
        {
            throw new NotImplementedException();
        }

        public AudioType Type { get; }
    }
}
