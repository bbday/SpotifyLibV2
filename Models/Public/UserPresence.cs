using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Public
{
    public class UserPresence
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("trackUri")]
        public string TrackUri { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long? Timestamp { get; set; }

        [JsonProperty("contextUri")]
        public string ContextUri { get; set; }

        [JsonProperty("contextIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int? ContextIndex { get; set; }
    }
}