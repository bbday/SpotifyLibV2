using System.Collections.Generic;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response
{
    public class ArtistsResponse
    {
        public List<FullArtist> Artists { get; set; } = default!;
    }
}

