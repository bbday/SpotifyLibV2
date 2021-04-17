using System;
using System.Linq;
using MediaLibrary.Enums;

namespace SpotifyLibrary.Ids
{
    public class UserId : StandardIdEquatable<UserId>
    {
        public UserId(string uri) : base(uri, uri.Split(':').Last(), AudioItemType.User, AudioServiceType.Spotify)
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
