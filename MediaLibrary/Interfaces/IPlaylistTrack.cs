using System;
using System.Windows.Input;

namespace MediaLibrary.Interfaces
{
    public interface IPlaylistTrack : IAlbumTrack
    {
        DateTime AddedOn { get; }
        string AddedBy { get; }
    }
}
