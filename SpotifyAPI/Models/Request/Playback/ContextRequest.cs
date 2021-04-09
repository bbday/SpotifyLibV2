namespace SpotifyLibrary.Models.Request.Playback
{
    public readonly struct ContextRequest : IPlayRequest
    {
        public ContextRequest(int trackIndex, bool repeatTrack, bool repeatContext, string contextUri)
        {
            TrackIndex = trackIndex;
            RepeatTrack = repeatTrack;
            RepeatContext = repeatContext;
            ContextUri = contextUri;
        }

        public int TrackIndex { get; }
        public bool RepeatTrack { get; }
        public bool RepeatContext { get; }
        public string ContextUri { get; }
        public string ContextUrl => $"context://{ContextUri}";

        public object GetModel()
        {
            return new
            {
                command = new
                {
                    context = new
                    {
                        metadata = new object(),
                        uri = ContextUri,
                        url = ContextUrl
                    },
                    endpoint = "play",
                    options = new
                    {
                        license = "premium",
                        player_options_override = new
                        {
                            repeating_context = RepeatContext,
                            repeating_track = RepeatTrack
                        },
                        skip_to = new
                        {
                            track_index = TrackIndex
                        }
                    },
                    play_origin = new
                    {
                        feature_identifier = "harmony",
                        feature_version = "4.13.0-6c0e4f7"
                    }
                }
            };
        }
    }
}