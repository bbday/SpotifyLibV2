
using System.Collections.Generic;
using System.Linq;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Connect.Audio
{
    public readonly struct ContextRequest : IPlayRequest
    {
        public ContextRequest(int trackIndex,
            ISpotifyId track,
            bool? repeatTrack, bool? repeatContext, bool? shuffle,
            string contextUri)
        {
            TrackIndex = trackIndex;
            RepeatTrack = repeatTrack;
            RepeatContext = repeatContext;
            ContextUri = contextUri;
            Shuffle = shuffle;
            Id = track;
        }

        public string TrackUri => Id.Uri;
        public int TrackIndex { get; }
        public bool? RepeatTrack { get; }
        public bool? RepeatContext { get; }
        public bool? Shuffle { get; }

        public string ContextUri { get; }
        public int PlayIndex => TrackIndex;
        public ISpotifyId Id { get; }
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
                        player_options_override = (RepeatContext != null || RepeatTrack != null)
                            ? new
                            {
                                repeating_context = RepeatContext,
                                repeating_track = RepeatTrack
                            }
                            : new object(),
                        skip_to = new
                        {
                            track_index = TrackIndex,
                            track_uri = TrackUri
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

    public readonly struct PagedRequest : IPlayRequest
    {
        public readonly IEnumerable<string> Uris;
        public string ContextUri { get; }
        public int PlayIndex { get; }
        public bool? RepeatTrack { get; }
        public bool? RepeatContext { get; }
        public ISpotifyId Id { get; }
        public PagedRequest(
            ISpotifyId itemId,
            string contextUri,
            IEnumerable<string> uris,
            int playIndex,
            bool repeatTrack,
            bool repeatContext,
            bool shuffle)
        {
            Id = itemId;
            Uris = uris;
            ContextUri = contextUri;
            PlayIndex = playIndex;
            RepeatTrack = repeatTrack;
            RepeatContext = repeatContext;
            Shuffle = shuffle;
        }

        public bool? Shuffle { get; }

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
                                tracks = Uris.Select(z => new
                                {
                                    metadata = new object(),
                                    uri = z
                                })
                            }
                        },
                        uri = ContextUri
                    },
                    endpoint = "play",
                    options = new
                    {
                        license = "premium",
                        skip_to = new
                        {
                            track_index = PlayIndex
                        },
                        player_options_override = new
                        {
                            repeating_context = RepeatContext,
                            repeating_track = RepeatTrack
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