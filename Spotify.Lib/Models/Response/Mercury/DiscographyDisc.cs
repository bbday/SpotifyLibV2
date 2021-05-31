using System.Collections.Generic;
using System.Text.Json.Serialization;
using Spotify.Lib.Models.TracksnEpisodes;

namespace Spotify.Lib.Models.Response.Mercury
{
    public struct DiscographyDisc
    {
        /// <summary>
        /// Number of the disc. (starts at 1)
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; set; }
        /// <summary>
        /// Name of the album (This is always null if you are fetching from artist)
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("tracks")]
        public IEnumerable<DiscographyTrack> Tracks { get; set; }
    }
}
