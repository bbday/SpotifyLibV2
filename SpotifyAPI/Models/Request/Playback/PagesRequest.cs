using System.Collections.Generic;
using System.Linq;

namespace SpotifyLibrary.Models.Request.Playback
{
    public readonly struct PagedRequest : IPlayRequest
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
            return new
            {
                command = new
                {
                    context = new
                    {
                        metadata = new object(),
                        pages = new List<object>
                        {
                            new
                            {
                                tracks = _uris.Select(z => new 
                                {
                                    metadata = new object(),
                                    uri = z
                                })
                            }
                        },
                        uri = _contextUri
                    },
                    endpoint = "play",
                    options = new 
                    {
                        license = "premium",
                        skip_to = new
                        {
                            track_index = _playIndex
                        },
                        player_options_override = new
                        {
                            repeating_context = _repeatContext,
                            repeating_track = _repeatTrack
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