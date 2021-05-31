using Spotify.Lib.Models;

namespace Spotify.Lib.Interfaces
{
    public interface ISpotifyId
    {
        string Uri { get; }
        string Id { get; }
        AudioItemType AudioType { get; }
        string ToMercuryUri(string locale);
        string ToHexId();
    }
}