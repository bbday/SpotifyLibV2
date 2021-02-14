using System.Text.Json.Serialization;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;


namespace SpotifyLibV2.Models.Request
{
    public partial class RemoteRequest
    {
        [J("command")] 
        public Command Command { get; set; }
    }

    public partial class Command
    {
        [J("context")] public Context Context { get; set; }
        [J("play_origin")] public PlayOrigin PlayOrigin { get; set; }
        [J("options")] public Options Options { get; set; }
        [J("endpoint")] public string Endpoint { get; set; }
    }

    public partial class Context
    {
        [J("uri")] public string Uri { get; set; }
        [J("url")] public string Url { get; set; }
        [J("metadata")] public Metadata Metadata { get; set; }
    }

    public partial class Metadata
    {
    }

    public partial class Options
    {
        [JsonPropertyName("license")]
        public string License { get; set; }
        [JsonPropertyName("skip_to")]
        public SkipTo SkipTo { get; set; }
        [JsonPropertyName("player_options_override")]
        public PlayerOptionsOverride PlayerOptionsOverride { get; set; }
    }

    public partial class PlayerOptionsOverride
    {
        [JsonPropertyName("repeating_track")]
        public bool RepeatingTrack { get; set; }
        [JsonPropertyName("repeating_context")]
        public bool RepeatingContext { get; set; }
    }

    public partial class SkipTo
    {
        [JsonPropertyName("track_index")] 
        public int TrackIndex { get; set; }
    }

    public class PlayOrigin
    {
        [JsonPropertyName("feature_identifier")]
        public string FeatureIdentifier { get; set; }
        [JsonPropertyName("feature_version")]
        public string FeatureVersion { get; set; }
    }
}