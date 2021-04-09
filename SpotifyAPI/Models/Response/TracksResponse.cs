using System.Collections.Generic;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response
{
    public class TracksResponse
    {
        public List<FullTrack> Tracks { get; set; } = default!;
    }
}