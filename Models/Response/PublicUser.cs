using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class Followers
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
    public class PublicUser
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; } = default!;

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public Followers Followers { get; set; } = default!;

        public string Href { get; set; } = default!;
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        public List<UserImage> Images { get; set; } = default!;

        public string Type { get; set; } = default!;
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = default!;
    }

    public class UserImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

