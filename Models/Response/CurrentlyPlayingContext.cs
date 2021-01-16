using Newtonsoft.Json;
using SpotifyLibV2.Helpers.Converters;

namespace SpotifyLibV2.Models.Response
{
    public class CurrentlyPlayingContext
    {
        public ApiDevice Device { get; set; } = default!;

        [JsonProperty("repeat_state")]
        public string RepeatState { get; set; } = default!;

        [JsonProperty("shuffle_state")]
        public bool ShuffleState { get; set; }

        public ApiContext Context { get; set; } = default!;

        public long Timestamp { get; set; }

        [JsonProperty("progress_ms")]
        public int ProgressMs { get; set; }

        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }

        [JsonConverter(typeof(PlayableItemConverter))]
        public GenericSpotifyItem Item { get; set; } = default!;

        public string CurrentlyPlayingType { get; set; } = default!;
    }
}