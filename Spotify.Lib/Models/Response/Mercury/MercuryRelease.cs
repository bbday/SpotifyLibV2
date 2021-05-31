using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Spotify.Lib.Models.Response.Mercury
{
    public class MercuryRelease : MercuryGenericRelease
    {
        /// <summary>
        /// Year of (digital) release.
        /// </summary>
        [JsonPropertyName("year")]
        public int Year { get; set; }

        /// <summary>
        /// Month of (digital) release.
        /// </summary>
        [JsonPropertyName("month")]
        public short? Month { get; set; }

        /// <summary>
        /// Day of (digital) release.
        [JsonPropertyName("day")]
        public short? Day { get; set; }

        /// <summary>
        /// Number of tracks inside the album.
        /// </summary>
        [JsonPropertyName("track_count")]
        public int TrackCount { get; set; }

        /// <summary>
        /// Basically the tracks of the album/single. May be null!
        /// </summary>
        [JsonPropertyName("discs")]
        public IEnumerable<DiscographyDisc>? Discs { get; set; }

        public override string Description => ReleaseDateAsDateTime.ToString("Y");

        public DateTime ReleaseDateAsDateTime => new DateTime(Year, Month ?? 1, Day ?? 1);
    }

}
