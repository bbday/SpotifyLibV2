using System;
using Spotify.Lib.Models.TracksnEpisodes;

namespace Spotify.Lib.Interfaces
{
    public interface IPlaylistTrack : IAlbumTrack
    {
        DateTime AddedOn { get; }
        string AddedBy { get; }
    }
}
