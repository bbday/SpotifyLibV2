using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Models.Response
{
    public class SimpleArtist : GenericSpotifyItem
    {
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public string Href { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}

