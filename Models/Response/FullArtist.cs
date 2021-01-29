using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class FullArtist : GenericSpotifyItem
    {
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        [JsonPropertyName("followers")]
        public Followers Followers { get; set; } = default!;
        [JsonPropertyName("genres")]
        public List<string> Genres { get; set; } = default!;
        public string Href { get; set; } = default!;
        [JsonPropertyName(("images"))]
        public List<SpotifyImage> Images { get; set; } = default!;
        [JsonPropertyName(("name"))]
        public string Name { get; set; } = default!;
        public int Popularity { get; set; }
    }
}