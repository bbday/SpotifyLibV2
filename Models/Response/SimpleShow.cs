using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class SimpleShow : GenericSpotifyItem
    {
        public List<string> AvailableMarkets { get; set; } = default!;


        public string Description { get; set; } = default!;

        public bool Explicit { get; set; }

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public string Href { get; set; } = default!;


        public List<SpotifyImage> Images { get; set; } = default!;

        public bool IsExternallyHosted { get; set; }


        public List<string> Languages { get; set; } = default!;

        public string MediaType { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Publisher { get; set; } = default!;

        public string Type { get; set; } = default!;
    }
}


