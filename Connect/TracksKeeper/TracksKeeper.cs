using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Connectstate;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
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
        public List<ContextTrack> Tracks = new List<ContextTrack>();
        private FisherYates<ContextTrack> shuffle = new FisherYates<ContextTrack>();
        private volatile bool isPlayingQueue = false;
        private volatile bool cannotLoadMore = false;
        private volatile int shuffleKeepIndex = -1;

        private readonly PlayerState _state;
        private readonly AbsSpotifyContext _context;
        private readonly LocalStateWrapper _stateWrapper;
        internal TracksKeeper(LocalStateWrapper stateWrapper,
            PlayerState state, 
            AbsSpotifyContext context)
        {
            _stateWrapper = stateWrapper;
            _state = state;
            _context = context;
        }
        public bool IsPlayingFirst() => _state.Index.Track == 0;
        public bool IsPlayingLast()
        {
            if (cannotLoadMore && !queue.Any()) return _state.Index.Track == Tracks.Count;
            return false;
        }
        public void InitializeFrom(Func<List<ContextTrack>, int> getIndex,
            ContextTrack track, Queue contextQueue)
        {
            Tracks.Clear();
            queue.Clear();

            while (true)
            {
                if (_stateWrapper.Pages.NextPage())
                {
                    var newTrack = _stateWrapper.Pages.CurrentPage();
                    var index = getIndex(Tracks);
                    if (index == -1)
                    {
                        Debug.WriteLine($"Could not find track, going to next page.");
                        Tracks.AddRange(newTrack);
                        continue;
                    }

                    index += Tracks.Count;
                    Tracks.AddRange(newTrack);

                    SetCurrentTrackIndex(index);
                    Debug.WriteLine($"Initialized current track index to {index}");
                    break;
                }
                else
                {
                    cannotLoadMore = true;
                    UpdateTrackCount();
                    throw new IllegalStateException("Couldn't find current track!");
                }
            }
        }
        public void InitializeStart()
        {
            if (!cannotLoadMore)
            {
                if (!_stateWrapper.Pages.NextPage()) throw new IllegalStateException();

                Tracks.Clear();
                Tracks.AddRange(_stateWrapper.Pages.CurrentPage());
            }

            CheckComplete();
            if (!PlayableId.CanPlaySomething(Tracks))
                throw new UnsupportedContextException("cannot play anything");

            var transformingShuffle = bool.Parse(
                _state.ContextMetadata.GetMetadataOrDefault("transforming.shuffle", "true"));
            if (_context.IsFinite() && _stateWrapper.IsShufflingContext
                                    && transformingShuffle) ShuffleEntirely();
            else _state.Options.ShufflingContext = false; // Must do this directly!

            SetCurrentTrackIndex(0);
        }

        public void ShuffleEntirely()
        {

        }
        public void ToggleShuffle(bool setTrue)
        {
            if (!_context.IsFinite()) throw new NotImplementedException("cannot shuffle an infinite context");
            if (Tracks.Count <= 1) return;
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

                shuffle.Shuffle(Tracks, true);
                shuffleKeepIndex = Tracks.FindIndex(z => z.Uri == currentlyPlaying.Uri);
                //TODO: Swap collections around pivot
                Tracks.Swap(0, shuffleKeepIndex);
                SetCurrentTrackIndex(0);
                Debug.WriteLine($"Shuffled context");
            }
            else
            {
                if (shuffle.CanUnshuffle(Tracks.Count))
                {
                    if (shuffleKeepIndex != -1)
                    {
                        Tracks.Swap(0, shuffleKeepIndex);
                    }
                    shuffle.Unshuffle(Tracks);
                    SetCurrentTrackIndex(Tracks.FindIndex(z => z.Uri == currentlyPlaying.Uri));
                    Debug.WriteLine($"Unshuffled fisher yates");
                }
                else
                {
                    Tracks.Clear();

                    //TODO: We need to create new pages

                    SetCurrentTrackIndex(Tracks.FindIndex(z => z.Uri == currentlyPlaying.Uri));
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
                _state.ContextMetadata.AddOrUpdate("track_count", (Tracks.Count + queue.Count).ToString());
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
                else cannotLoadMore = totalTracks == Tracks.Count;
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
                var itemAtIndex = Tracks[(int) _state.Index.Track];
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
                _state.PrevTracks.Add(ProtoUtils.ConvertToProvidedTrack(Tracks[i], _state.ContextUri));
            }


            _state.NextTracks.Clear();
            for (int i = index + 1; i < Math.Min(Tracks.Count, index + 1 + MAX_PREV_TRACKS); i++)
            {
                _state.NextTracks.Add(ProtoUtils.ConvertToProvidedTrack(Tracks[i], _state.ContextUri));
            }
        }
    }
}
