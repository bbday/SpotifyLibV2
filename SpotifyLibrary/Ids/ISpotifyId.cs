
using MediaLibrary.Interfaces;

namespace SpotifyLibrary.Ids
{
    public interface ISpotifyId : IAudioId
    {
        string Uri { get;  }
        string Id { get; }
        string ToMercuryUri(string locale);
        string ToHexId();
    }
}
