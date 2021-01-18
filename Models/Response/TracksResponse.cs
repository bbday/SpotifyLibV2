using System.Collections.Generic;

namespace SpotifyLibV2.Models.Response
{
    public class TracksResponse
    {
        public List<FullTrack> Tracks { get; set; } = default!;
    }
}