using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Ids
{
    public interface ISpotifyDescription : ISpotifyId
    {
        string Title { get; }
    }
    public interface ISpotifyId
    {
        string Uri { get; }
        string Id { get; }
        string ToHexId();
        string ToMercuryUri();
        SpotifyType Type { get; }
    }
}
