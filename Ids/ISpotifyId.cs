using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Ids
{
    public interface ISpotifyDescription : ISpotifyId
    {
        string Title { get; }
    }
    public interface ISpotifyId : IAudioId
    {
        string Uri { get; }
        string Id { get; }
        string ToHexId();
        string ToMercuryUri();
        AudioType Type { get; }
    }
}
