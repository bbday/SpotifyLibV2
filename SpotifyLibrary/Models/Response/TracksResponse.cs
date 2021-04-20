using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response
{
    public class TracksResponse
    {
        public List<FullTrack> Tracks { get; set; } = default!;
    }
    public class EpisodesResponse
    {
        public List<FullEpisode> Episodes { get; set; } = default!;
    }
}