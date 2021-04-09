using System.Collections.Generic;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers.JsonConverters;
using SpotifyLibrary.Models.Playlists;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullPlaylist : SimplePlaylist
    {
        public Followers Followers { get; set; }

        public PlaylistTracksObject Tracks { get; set; }
    }

    public class PlaylistTracksObject
    {
        public List<SimplePlaylistTrack> Items { get; set; }
    }

    public class SimplePlaylistTrack
    {
      public FullTrack Track { get; set; }
    }
}
