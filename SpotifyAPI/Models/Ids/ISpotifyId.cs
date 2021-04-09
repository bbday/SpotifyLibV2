
using MusicLibrary.Interfaces;

namespace SpotifyLibrary.Models.Ids
{
    public interface ISpotifyId : IAudioId
    {
        string Uri { get;  }
        string Id { get; }
        string ToMercuryUri(string locale);
        string ToHexId();
    }
}
