namespace SpotifyLibV2.Models.Request.PlaybackRequests
{
    public readonly struct ContextRequest : IPlaybackRequest
    {
        private readonly  string _contextUri;
        private readonly int _contextIndex;
        public ContextRequest(
            string contextUri, 
            int contextIndex)
        {
            _contextUri = contextUri;
            _contextIndex = contextIndex;
        }
        public object GetModel()
        {
            return new RemoteRequest
            {
                Command = new Command
                {
                    Context = new Context
                    {
                        Metadata = new Metadata(),
                        Uri = _contextUri,
                        Url = $"context://{_contextUri}"
                    },
                    Endpoint = "play",
                    Options = new Options
                    {
                        License = "premium",
                        PlayerOptionsOverride = new PlayerOptionsOverride
                        {
                            RepeatingContext = false, //TODO
                            RepeatingTrack = false //TODO
                        },
                        SkipTo = new SkipTo
                        {
                            TrackIndex = _contextIndex
                        }
                    },
                    PlayOrigin = new PlayOrigin
                    {
                        FeatureIdentifier = "harmony",
                        FeatureVersion = "4.9.0-d242618"
                    }
                }
            };
        }
    }
}
