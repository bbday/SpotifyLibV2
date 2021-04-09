using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public struct ResumePoint
    {
        [JsonProperty("resume_position_ms")]
        public long ResumePositionMs { get; set; }
        [JsonProperty("fully_played")]
        public bool FullyPlayed { get; set; }
    }
}
