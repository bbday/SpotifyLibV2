using System.Text.Json.Serialization;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class PlaycountedTrack : GenericSpotifyTrack
    {
        /// <summary>
        /// Number of plays. If the value is null then spotify displays "< 1000"
        /// </summary>
        [JsonPropertyName("playcount")]
        public long? Playcount { get; set; }
    }

}
