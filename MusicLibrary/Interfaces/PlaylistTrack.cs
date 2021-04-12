using System;
using System.Windows.Input;

namespace MusicLibrary.Interfaces
{
    public interface IPlaylistTrack : IAlbumTrack
    {
        DateTime AddedOn { get;  }
        string AddedBy { get; }

        ICommand PlayCommand { get; set; }
    }
}
