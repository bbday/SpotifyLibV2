using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Public
{
    public class UserPresence
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("trackUri")]
        public string TrackUri { get; set; }

        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }

        [JsonPropertyName("contextUri")]
        public string ContextUri { get; set; }

        [JsonPropertyName("contextIndex")]
        public int? ContextIndex { get; set; }
    }
}