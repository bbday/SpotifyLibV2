namespace Spotify.Lib.Interfaces
{
    public interface IPlayRequest
    {
        bool? RepeatContext { get; }
        bool? RepeatTrack { get; }
        bool? Shuffle { get; }
        string ContextUri { get; }
        int PlayIndex { get; }
        ISpotifyId Id { get; }
        object GetModel();
    }
}