using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Shared
{
    public class SpotifyImage
    {
        [JsonProperty("height")]
        public double? Height { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public double? Width { get; set; }
    }
}