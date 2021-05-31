using System;
using System.Linq;

namespace Spotify.Lib.Models.Ids
{
    public class UserId : StandardIdEquatable<UserId>
    {
        public UserId(string uri) : base(uri, uri.Split(':').Last(), AudioItemType.User)
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