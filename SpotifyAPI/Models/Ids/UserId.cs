using System;
using System.Linq;
using MusicLibrary.Enum;

namespace SpotifyLibrary.Models.Ids
{
    public class UserId : StandardIdEquatable<UserId>
    {
        public UserId(string uri) : base(uri, uri.Split(':').Last(), AudioType.User, AudioService.Spotify)
        {
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
