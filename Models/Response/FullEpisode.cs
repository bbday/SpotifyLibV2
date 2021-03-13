using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Interfaces;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class FullEpisode : GenericSpotifyItem, IPlayableItem
    {
        public string AudioPreviewUrl { get; set; } = default!;
        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string Description { get; set; } = default!;

        [JsonPropertyName("duration_ms")]
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }

        public bool Explicit { get; set; }

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public string Href { get; set; } = default!;


        public List<SpotifyImage> Images { get; set; } = default!;

        public bool IsExternallyHosted { get; set; }

        public bool IsPlayable { get; set; }

        public List<string> Languages { get; set; } = default!;
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        public string ReleaseDate { get; set; } = default!;

        public string ReleaseDatePrecision { get; set; } = default!;
        [JsonPropertyName("resume_point")]
        [JsonProperty("resume_point")]
        public ResumePoint ResumePoint { get; set; } = default!;
        [JsonPropertyName("show")]
        public SimpleShow Show { get; set; } = default!;

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public AudioType Type { get; set; }

        public PlaylistType DerivedFromList { get; set; }
    }
}
