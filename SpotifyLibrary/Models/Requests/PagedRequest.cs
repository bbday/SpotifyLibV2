using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotifyLibrary.Models.Requests
{
    public readonly struct PagedRequest 
    {
        public readonly IEnumerable<string> Uris;
        public readonly string ContextUri;
        public readonly int PlayIndex;

        public readonly bool RepeatTrack;
        public readonly bool RepeatContext;

        public PagedRequest(
            string contextUri,
            IEnumerable<string> uris,
            int playIndex,
            bool repeatTrack,
            bool repeatContext,
            bool shuffle)
        {
            Uris = uris;
            ContextUri = contextUri;
            PlayIndex = playIndex;
            RepeatTrack = repeatTrack;
            RepeatContext = repeatContext;
            Shuffle = shuffle;
        }

        public bool Shuffle { get; }

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