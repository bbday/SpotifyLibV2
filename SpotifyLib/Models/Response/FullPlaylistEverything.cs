using Spotify.Playlist4.Proto;
using SpotifyLib.Models.Response.SimpleItems;

namespace SpotifyLib.Models.Response
{
    public readonly struct FullPlaylistEverything
    {
        public FullPlaylistEverything(SelectedListContent selectedListContent, 
            SimplePlaylist playlistMetadata)
        {
            SelectedListContent = selectedListContent;
            PlaylistMetadata = playlistMetadata;
        }

        /// <summary>
        /// Contains data such as: Header Image, but mainly it contains all the track URIs.
        /// </summary>
        public SelectedListContent SelectedListContent { get; }
        public SimplePlaylist PlaylistMetadata { get; }
    }
}
