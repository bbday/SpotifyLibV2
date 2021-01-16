using System.Collections.Generic;
using Newtonsoft.Json;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class MercuryAlbum : GenericSpotifyItem
    {
        private List<Disc> _discs;
        [JsonProperty("related")]
        public Related Related { get; set; }
        [JsonIgnore]
        public string Description { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public long TrackCount { get; set; }

        [JsonProperty("discs")]
        public List<Disc> Discs
        {
            get => _discs;
            set { _discs = value; }
        }
        [JsonProperty("artists")]
        public List<MercuryAlbumArtist> Artists { get; set; }
        public long Month { get; set; }

        [JsonProperty("day")]
        public long Day { get; set; }

        [JsonProperty("additional", NullValueHandling = NullValueHandling.Ignore)]
        public AdditionalReleases Additional { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("copyrights")]
        public List<string> Copyrights { get; set; }
        [JsonIgnore]
        public string DerivedFrom { get; set; }
    }

    public class AdditionalReleases
    {
        [JsonProperty("releases")]
        public List<Release> Releases { get; set; }
    }
    public class MercuryAlbumArtist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
    public class AlbumTrack : Track
    {

        [JsonProperty("popularity")]
        public long Popularity { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("explicit")]
        public bool Explicit { get; set; }

        [JsonProperty("playable")]
        public bool Playable { get; set; }
    }
    public partial class TrackArtist
    {

        [JsonProperty("image")]
        public SpotifyCover Image { get; set; }
    }
    public class Related
    {
        [JsonProperty("releases")]
        public List<Release> Releases { get; set; }
    }

    public class Release : GenericSpotifyItem
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public SpotifyCover Cover { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("track_count")]
        public long TrackCount { get; set; }

        [JsonProperty("month")]
        public long Month { get; set; }

        [JsonProperty("day")]
        public long Day { get; set; }
    }
}