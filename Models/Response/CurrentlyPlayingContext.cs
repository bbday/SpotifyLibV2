using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SpotifyLibV2.Helpers.Converters;

namespace SpotifyLibV2.Models.Response
{
    public class CurrentlyPlayingContext
    {
        [JsonProperty("device")]
        [JsonPropertyName("device")]
        public Device Device { get; set; } = default!;

        [JsonProperty("repeat_state")]
        [JsonPropertyName("repeat_state")]
        public string RepeatState { get; set; } = default!;

        [JsonPropertyName("shuffle_state")]
        [JsonProperty("shuffle_state")]
        public bool ShuffleState { get; set; }
        [JsonPropertyName("context")]
        [JsonProperty("context")]
        public ApiContext Context { get; set; } = default!;

        [JsonPropertyName("timestamp")]
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("progress_ms")]
        [JsonProperty("progress_ms")]
        public int ProgressMs { get; set; }

        [JsonPropertyName("is_playing")]
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(PlayableItemConverter))]

        [Newtonsoft.Json.JsonConverter(typeof(PlayableItemConverter))]
        public GenericSpotifyItem Item { get; set; } = default!;

        public string CurrentlyPlayingType { get; set; } = default!;
    }
}