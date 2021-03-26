using System.Linq;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
    public class EpisodeId : StandardIdEquatable<EpisodeId>
    {
        public EpisodeId(string uri) :
            base(uri, uri.Split(':').Last(), AudioType.Episode, AudioService.Spotify)
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
