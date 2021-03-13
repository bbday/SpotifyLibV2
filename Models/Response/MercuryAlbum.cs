using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class MercuryAlbum : GenericSpotifyItem
    {
        private List<DiscographyDisc> _discs;
        [JsonProperty("related")]
        [JsonPropertyName("related")]
        public Related Related { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string Description { get; set; }
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("cover")]
        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }

        [JsonPropertyName("year")]
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonPropertyName("track_count")]
        [JsonProperty("track_count")]
        public long TrackCount { get; set; }

        /// <summary>
        /// Basically the tracks of the album/single. May be null!
        /// </summary>
        [JsonPropertyName("discs")]
        public IEnumerable<DiscographyDisc>? Discs { get; set; }

        [JsonPropertyName("artists")]
        [JsonProperty("artists")]
        public List<MercuryAlbumArtist> Artists { get; set; }
        [JsonPropertyName("month")]
        [JsonProperty("month")]
        public long Month { get; set; }

        [JsonProperty("day")]
        [JsonPropertyName("day")]
        public long Day { get; set; }

        [JsonPropertyName("additional")]
        [JsonProperty("additional", NullValueHandling = NullValueHandling.Ignore)]
        public AdditionalReleases Additional { get; set; }
        [JsonProperty("label")]
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonProperty("copyrights")]
        [JsonPropertyName("copyrights")]
        public List<string> Copyrights { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string DerivedFrom { get; set; }
        [JsonPropertyName("type")]
        public new string Type { get; set; }
    }

    public class AdditionalReleases
    {
        [JsonProperty("releases")]
        [JsonPropertyName("releases")]
        public List<ArtistDiscographyRelease> Releases { get; set; }
    }
    public class MercuryAlbumArtist
    {
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonPropertyName("uri")]
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
    public class AlbumTrack : Track
    {

        [JsonPropertyName("popularity")]
        [JsonProperty("popularity")]
        public long Popularity { get; set; }

        [JsonPropertyName("number")]
        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonPropertyName("explicit")]
        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonPropertyName("playable")]
        [JsonProperty("playable")]
        public bool Playable { get; set; }
    }
    public partial class TrackArtist
    {
        [JsonPropertyName("image")]
        [JsonProperty("image")]
        public SpotifyCover Image { get; set; }
    }
    public class Related
    {
        [JsonPropertyName("releases")]
        [JsonProperty("releases")]
        public List<ArtistDiscographyRelease> Releases { get; set; }
    }

}