﻿using System.Collections.Generic;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class FullArtist : GenericSpotifyItem
    {
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public Followers Followers { get; set; } = default!;
        public List<string> Genres { get; set; } = default!;
        public string Href { get; set; } = default!;
        public List<SpotifyImage> Images { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Popularity { get; set; }
        public string Type { get; set; } = default!;
    }
}