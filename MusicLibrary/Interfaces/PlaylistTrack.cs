using System;

namespace MusicLibrary.Interfaces
{
    public interface IPlaylistTrack : IAlbumTrack
    {
        DateTime AddedOn { get;  }
        string AddedBy { get; }
    }
}
