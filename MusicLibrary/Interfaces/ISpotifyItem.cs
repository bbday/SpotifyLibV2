namespace MusicLibrary.Interfaces
{
    public interface ISpotifyItem : IAudioItem
    {
        string Uri { get;  }
    }
}
