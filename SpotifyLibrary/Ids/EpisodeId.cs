using System.Linq;
using MediaLibrary.Enums;

namespace SpotifyLibrary.Ids
{
    public class EpisodeId : StandardIdEquatable<EpisodeId>
    {
        public EpisodeId(string uri) :
            base(uri, uri.Split(':').Last(), AudioItemType.Episode, AudioServiceType.Spotify)
        {

        }

        public override string ToMercuryUri(string locale)
        {
            throw new System.NotImplementedException();
        }

        public override string ToHexId()
        {
            throw new System.NotImplementedException();
        }
    }
}
