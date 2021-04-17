using MediaLibrary.Interfaces;

namespace SpotifyLibrary.Interfaces
{
    public interface ISpotifyItem : IAudioItem
    {
        string Uri { get; }
    }
}
