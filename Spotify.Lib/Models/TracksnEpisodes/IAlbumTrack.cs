namespace Spotify.Lib.Models.TracksnEpisodes
{
    public interface IAlbumTrack : ITrackItem
    {
        AlbumType AlbumType { get; }
    }
}
