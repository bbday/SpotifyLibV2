using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class SimpleArtist : GenericSpotifyItem
    {
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public string Href { get; set; } = default!;
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }
}

