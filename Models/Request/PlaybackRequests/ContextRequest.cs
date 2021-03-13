using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SpotifyLibV2.Models.Request.PlaybackRequests
{

    public readonly struct TransferRequst : IPlaybackRequest
    {
        public object GetModel()
        {
            return new PagedRequest
            {
                TransferOptions = new TransferOptions
                {
                    RestorePaused = "restore"
                }
            };
        }
        private class PagedRequest
        {
            [JsonPropertyName("transfer_options")]
            public TransferOptions TransferOptions { get; set; }
        }

        private class TransferOptions
        {
            [JsonPropertyName("restore_paused")]
            public string RestorePaused { get; set; }
        }
    }
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

    public readonly struct PagedRequest : IPlaybackRequest
    {
        private readonly IEnumerable<string> _uris;
        private readonly string _contextUri;
        private readonly int _playIndex;

        private readonly bool _repeatTrack;
        private readonly bool _repeatContext;
        public PagedRequest(
            string contextUri, 
            IEnumerable<string> uris,
            int playIndex, 
            bool repeatTrack, 
            bool repeatContext)
        {
            _uris = uris;
            _contextUri = contextUri;
            _playIndex = playIndex;
            _repeatTrack = repeatTrack;
            _repeatContext = repeatContext;
        }
        public object GetModel()
        {
            return new RequestData
            {
                Command = new RequestCommand
                {
                    Context = new RequestContext
                    {
                        Metadata = new object(),
                        Pages = new List<RequestContextPage>
                        {
                            new()
                            {
                                Tracks = _uris.Select(z => new ContextPageTrack
                                {
                                    Metadata = new object(),
                                    Uri = z
                                })
                            }
                        },
                        Uri = _contextUri
                    },
                    Endpoint = "play",
                    Options = new Options
                    {
                        License = "premium",
                        SkipTo = new SkipTo
                        {
                            TrackIndex = _playIndex
                        },
                        PlayerOptionsOverride = new PlayerOptionsOverride
                        {
                            RepeatingContext = _repeatContext,
                            RepeatingTrack = _repeatTrack
                        }
                    },
                    Origin = new PlayOrigin
                    {
                        FeatureIdentifier = "harmony",
                        FeatureVersion = "4.11.0-af0ef98"
                    }
                }
            };
        }

        private class RequestData
        {
            [JsonPropertyName("command")]
            public RequestCommand Command { get; set; }
        }

        private class RequestCommand
        {
            [JsonPropertyName("context")]
            public RequestContext Context { get; set; }
            [JsonPropertyName("play_origin")]
            public PlayOrigin Origin { get; set; }
            [JsonPropertyName("options")]
            public Options Options { get; set; }
            [JsonPropertyName("endpoint")]
            public string Endpoint { get; set; }
        }

        private class RequestContext
        {
            [JsonPropertyName("uri")]
            public string Uri { get; set; }
            [JsonPropertyName("metadata")]
            public object Metadata { get; set; }
            [JsonPropertyName("pages")]
            public IEnumerable<RequestContextPage> Pages { get; set; }
        }

        private class RequestContextPage
        {
            [JsonPropertyName("tracks")]
            public IEnumerable<ContextPageTrack> Tracks { get; set; }
        }
        private class ContextPageTrack
        {
            [JsonPropertyName("uri")]
            public string Uri { get; set; }
            [JsonPropertyName("metadata")]
            public object Metadata { get; set; }
        }
    }
}
