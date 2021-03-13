using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Connectstate;
using Spotify.Player.Proto;
using SpotifyLibV2.Connect.Contexts;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Connect.TracksKeeper
{
    public class TracksKeeper
    {
        public static short MAX_PREV_TRACKS = 16;
        public static short MAX_NEXT_TRACKS = 48;

        private LinkedList<ContextTrack> queue = new LinkedList<ContextTrack>();
        private List<ContextTrack> tracks = new List<ContextTrack>();
        private FisherYates<ContextTrack> shuffle = new FisherYates<ContextTrack>();
        private volatile bool isPlayingQueue = false;
        private volatile bool cannotLoadMore = false;
        private volatile int shuffleKeepIndex = -1;

        private readonly PlayerState _state;
        private readonly AbsSpotifyContext _context;
        public TracksKeeper(PlayerState state, 
            AbsSpotifyContext context)
        {
            _state = state;
            _context = context;
        }


        public void ToggleShuffle(bool setTrue)
        {
            if (!_context.IsFinite()) throw new NotImplementedException("cannot shuffle an infinite context");
            if (tracks.Count <= 1) return;
            if (isPlayingQueue) return;

            var currentlyPlaying = PlayableId.FromUri(_state.Track.Uri);

            if (setTrue)
            {
                if (!cannotLoadMore)
                {
                    if (LoadAllTracks())
                    {
                        Debug.WriteLine($"Loaded all tracks before shuffling");
                    }
                    else
                    {
                        Debug.WriteLine($"Cannot shuffle context for unknown reason");
                        return;
                    }
                }

                shuffle.Shuffle(tracks, true);
                shuffleKeepIndex = tracks.FindIndex(z => z.Uri == currentlyPlaying.Uri);
                //TODO: Swap collections around pivot
                tracks.Swap(0, shuffleKeepIndex);
                SetCurrentTrackIndex(0);
                Debug.WriteLine($"Shuffled context");
            }
            else
            {
                if (shuffle.CanUnshuffle(tracks.Count))
                {
                    if (shuffleKeepIndex != -1)
                    {
                        tracks.Swap(0, shuffleKeepIndex);
                    }
                    shuffle.Unshuffle(tracks);
                    SetCurrentTrackIndex(tracks.FindIndex(z => z.Uri == currentlyPlaying.Uri));
                    Debug.WriteLine($"Unshuffled fisher yates");
                }
                else
                {
                    tracks.Clear();

                    //TODO: We need to create new pages

                    SetCurrentTrackIndex(tracks.FindIndex(z => z.Uri == currentlyPlaying.Uri));
                    Debug.WriteLine($"Unshuffled fisher yates by reloading context");
                }
            }
        }

        public bool LoadAllTracks()
        {
            if (!_context.IsFinite()) throw new IllegalStateException();
            try
            {
                while (true)
                {
                    //paging
                }
            }
            catch (Exception x)
            {
                switch (x)
                {
                    case IOException _:
                    case MercuryException _:
                        Debug.WriteLine($"Failed loading all tracks {x}");
                        //Somehow notify the UI applications(receivers)
                        return false;
                        break;
                }
            }

            cannotLoadMore = true;
            UpdateTrackCount();

            return true;
        }
        public void UpdateTrackCount()
        {
            if (_context.IsFinite())
                _state.ContextMetadata.AddOrUpdate("track_count", (tracks.Count + queue.Count).ToString());
            else
                _state.ContextMetadata.Remove("track_count");
        }

        public void CheckComplete()
        {
            if (cannotLoadMore) return;

            if (_context.IsFinite())
            {
                var totalTracks = int.Parse(_state.ContextMetadata.GetMetadataOrDefault("track_count", "-1"));
                if (totalTracks == -1) cannotLoadMore = false;
                else cannotLoadMore = totalTracks == tracks.Count;
            }
            else
                cannotLoadMore = false;
        }

        public void SetCurrentTrackIndex(int index)
        {
            if (isPlayingQueue) throw new IllegalStateException();
            _state.Index = new ContextIndex
            {
                Track = (uint) index
            };
            UpdateState();
        }

        public void UpdateState()
        {
            if (isPlayingQueue)
            {
                var head = queue.First;
                _state.Track = ProtoUtils.ConvertToProvidedTrack(head.Value, _state.ContextUri);
                queue.Remove(head);
            }
            else
            {
                var itemAtIndex = tracks[(int) _state.Index.Track];
                _state.Track = ProtoUtils.ConvertToProvidedTrack(itemAtIndex, _state.ContextUri);
            }

            UpdateLikeDislike();
            UpdateTrackDuration();
            UpdatePrevNextTracks();
        }

        public void UpdateLikeDislike()
        {
            if (string.Equals(_state.ContextMetadata.GetMetadataOrDefault("like-feedback-enabled", "0"), "1"))
            {
                _state.ContextMetadata.AddOrUpdate("like-feedback-selected",
                    _state.Track.Metadata.GetMetadataOrDefault("like-feedback-selected", "0"));
            }
            else
            {
                _state.ContextMetadata.Remove("like-feedback-selected");
            }

            if (string.Equals(_state.ContextMetadata.GetMetadataOrDefault("dislike-feedback-enabled", "0"), "1"))
            {
                _state.ContextMetadata.AddOrUpdate("dislike-feedback-selected",
                    _state.Track.Metadata.GetMetadataOrDefault("dislike-feedback-selected", "0"));
            }
            else
            {
                _state.ContextMetadata.Remove("dislike-feedback-selected");
            }
        }

        public void UpdateTrackDuration()
        {
            var current = _state.Track;
            _state.Duration = current.Metadata.ContainsKey("duration")
                ? long.Parse(current.Metadata["duration"])
                : 0L;
        }

        public void UpdatePrevNextTracks()
        {
            var index = (int)_state.Index.Track;

            _state.PrevTracks.Clear();
            for (int i = Math.Max(0, index - MAX_PREV_TRACKS); i < index; i++)
            {
                _state.PrevTracks.Add(ProtoUtils.ConvertToProvidedTrack(tracks[i], _state.ContextUri));
            }


            _state.NextTracks.Clear();
            for (int i = index + 1; i < Math.Min(tracks.Count, index + 1 + MAX_PREV_TRACKS); i++)
            {
                _state.NextTracks.Add(ProtoUtils.ConvertToProvidedTrack(tracks[i], _state.ContextUri));
            }
        }
    }
}
