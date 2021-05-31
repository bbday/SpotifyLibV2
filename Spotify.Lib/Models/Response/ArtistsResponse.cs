using System.Collections.Generic;
using Spotify.Lib.Models.Response.SpotItems.FullItems;

namespace Spotify.Lib.Models.Response
{
    public struct ArtistsResponse
    {
        public List<FullArtist> Artists { get; set; }
    }
    public struct AlbumsResponse
    {
        public List<FullAlbum> Albums { get; set; }
    }
    public struct TracksResponse
    {
        public List<FullTrack?> Tracks { get; set; }
    }
}

