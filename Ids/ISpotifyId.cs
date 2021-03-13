using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Ids
{
    public interface ISpotifyId : IAudioId
    {
        string Uri { get; }
        string Id { get; }
        string ToHexId();
        string ToMercuryUri();
        AudioType Type { get; }
    }
}
