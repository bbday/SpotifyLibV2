using System;
using System.Linq;

namespace Spotify.Lib.Models.Ids
{
    public class EpisodeId : StandardIdEquatable<EpisodeId>
    {
        public EpisodeId(string uri) :
            base(uri, uri.Split(':').Last(), AudioItemType.Episode)
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