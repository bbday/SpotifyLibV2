#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class FullPlaylist : GenericSpotifyItem
    {
        [JsonPropertyName("collaborative")]
        public bool? Collaborative { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; } = default!;
        public Dictionary<string, string>? ExternalUrls { get; set; } = default!;
        [JsonPropertyName("followers")]
        public Followers Followers { get; set; } = default!;
        public string? Href { get; set; } = default!;
        [JsonPropertyName("images")]
        public List<SpotifyImage>? Images { get; set; } = default!;
        public string? Name { get; set; } = default!;
        [JsonProperty("owner")]
        public PublicUser? Owner { get; set; } = default!;
        [JsonProperty("public")]
        public bool? Public { get; set; }
        [JsonProperty("snapshot_id")]
        [JsonPropertyName("snapshot_id")]
        public string? SnapshotId { get; set; } = default!;
    }
}