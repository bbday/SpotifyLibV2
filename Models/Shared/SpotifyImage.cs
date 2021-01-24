using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Shared
{
    public class SpotifyImage
    {
        [JsonPropertyName("height")]
        [JsonProperty("height")]
        public double? Height { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonPropertyName("width")]
        [JsonProperty("width")]
        public double? Width { get; set; }
    }
}