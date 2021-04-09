using System.Collections.Generic;
using MusicLibrary.Interfaces;

namespace SpotifyLibrary.Models.Playlists
{
    public interface IPlaylistHeader
    {
        IAudioId Id { get; }
        string Title { get; set; }
        string Description { get; set; }
        string TopString { get; set; }
        ICollection<object> HorizontalCaptionItems { get; set; }
        string MainCoverString { get; set; }
    }
}

