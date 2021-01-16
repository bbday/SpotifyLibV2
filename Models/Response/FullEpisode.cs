using System;
using System.Collections.Generic;
using System.Text;
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

        public string Description { get; set; } = default!;

        public int DurationMs { get; set; }

        public bool Explicit { get; set; }

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public string Href { get; set; } = default!;


        public List<SpotifyImage> Images { get; set; } = default!;

        public bool IsExternallyHosted { get; set; }

        public bool IsPlayable { get; set; }

        public List<string> Languages { get; set; } = default!;

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        public string ReleaseDate { get; set; } = default!;

        public string ReleaseDatePrecision { get; set; } = default!;

        public ResumePoint ResumePoint { get; set; } = default!;

        public SimpleShow Show { get; set; } = default!;

        [JsonConverter(typeof(StringEnumConverter))]
        public SpotifyType Type { get; set; }

        public PlaylistType DerivedFromList { get; set; }
    }
}
