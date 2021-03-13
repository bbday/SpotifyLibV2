using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SpotifyLib.Models.Api.Paging;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class FullAlbum : GenericSpotifyItem
    {
        [JsonProperty("album_type")]
        public string AlbumType { get; set; } = default!;
        public List<SimpleArtist> Artists { get; set; } = default!;
        public List<string> AvailableMarkets { get; set; } = default!;
        public Dictionary<string, string> ExternalIds { get; set; } = default!;
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public List<string> Genres { get; set; } = default!;
        public string Href { get; set; } = default!;
        public List<SpotifyImage> Images { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Name { get; set; } = default!;
        public int Popularity { get; set; }
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; } = default!;
        public string ReleaseDatePrecision { get; set; } = default!;
        public Dictionary<string, string> Restrictions { get; set; } = default!;
        public string Type { get; set; } = default!; 
        [JsonPropertyName("tracks")]
        [JsonProperty("tracks")]
        public Paging<SimpleTrack> Tracks { get; set; } = default!;
    }

    public class SimpleTrack
    {
        [JsonProperty("artists")]
        [JsonPropertyName("track_number")]
        public List<SimpleArtist> Artists { get; set; } = default!;
        public List<string> AvailableMarkets { get; set; } = default!;
        [JsonProperty("disc_number")]
        [JsonPropertyName("track_number")]
        public int DiscNumber { get; set; }
        [JsonProperty("duration_ms")]
        [JsonPropertyName("track_number")]
        public int DurationMs { get; set; }
        [JsonProperty("explicit")]
        [JsonPropertyName("track_number")]
        public bool Explicit { get; set; }
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public string Href { get; set; } = default!;
        [JsonProperty("id")]
        [JsonPropertyName("track_number")]
        public string Id { get; set; } = default!;
        public bool IsPlayable { get; set; }
        [JsonProperty("name")]
        [JsonPropertyName("track_number")]
        public string Name { get; set; } = default!;
        public string PreviewUrl { get; set; } = default!;
        [JsonProperty("track_number")]
        [JsonPropertyName("track_number")]
        public int TrackNumber { get; set; }
        [JsonProperty("uri")]
        [JsonPropertyName("track_number")]
        public string Uri { get; set; } = default!;
    }
}