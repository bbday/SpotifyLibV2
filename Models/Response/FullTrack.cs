using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Interfaces;

namespace SpotifyLibV2.Models.Response
{
    public class FullTrack : GenericSpotifyItem, IComparable<FullTrack>, IComparable, IPlayableItem
    {
        [JsonPropertyName("is_saved")]
        public bool IsSaved { get; set; }
        [JsonPropertyName("album")]
        public SimpleAlbum Album { get; set; } = default!;
        [JsonPropertyName("artists")]
        public List<SimpleArtist> Artists { get; set; } = default!;
        public List<string> AvailableMarkets { get; set; } = default!;
        [JsonPropertyName("disc_number")]
        public int DiscNumber { get; set; }
        [JsonProperty("duration_ms")]
        [JsonPropertyName("duration_ms")]
        public long DurationMs { get; set; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        public Dictionary<string, string> ExternalIds { get; set; } = default!;
        public Dictionary<string, string> ExternalUrls { get; set; } = default!;
        public string Href { get; set; } = default!;
        [JsonProperty("is_playable")]
        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; }
        public Dictionary<string, string> Restrictions { get; set; } = default!;
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        public int Popularity { get; set; }
        public string PreviewUrl { get; set; } = default!;
        public int TrackNumber { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public DateTime AddedAt { get; set; }
        [JsonProperty("is_local")]
        [JsonPropertyName("is_local")]
        public bool IsLocal { get; set; }
        public int CompareTo(object obj)
        {
            return ((new CaseInsensitiveComparer()).Compare(Id, (obj as FullTrack).Id));
        }

        public int CompareTo(FullTrack other)
        {
            return ((new CaseInsensitiveComparer()).Compare(Id, other.Id));
        }

        public PlaylistType DerivedFromList { get; set; }
    }
}


