using System.Collections.Generic;
using System.Text.Json.Serialization;
using Spotify.Lib.Models.Response.SpotItems;

namespace Spotify.Lib.Models.TracksnEpisodes
{
    public class PlaycountedTrack : GenericSpotifyTrack
    {
        /// <summary>
        ///     Number of plays. If the value is null then spotify displays "< 1000"
        /// </summary>
        [JsonPropertyName("playcount")]
        public long? Playcount { get; set; }

        public override string Description { get; set; }
        public override string Caption { get; }
        public override List<UrlImage> Images { get; set; }
    }
}