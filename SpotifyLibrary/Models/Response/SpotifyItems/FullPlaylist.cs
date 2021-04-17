using System.Collections.Generic;

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
    public class Followers
    {
        public int Total { get; set; }
    }
}
