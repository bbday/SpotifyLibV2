using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Connectstate;
using JetBrains.Annotations;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLibrary.Connect.Contexts;
using SpotifyLibrary.Connect.Exceptions;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.Extensions;
using UnsupportedContextException = SpotifyLibrary.Connect.Contexts.UnsupportedContextException;

namespace SpotifyLibrary.Connect.TracksKeepers
{
    public class TracksKeeper
    {
        public static short MAX_PREV_TRACKS = 16;
        public static short MAX_NEXT_TRACKS = 48;

        public LinkedList<ContextTrack> Queue = new LinkedList<ContextTrack>();
        public List<ContextTrack> Tracks = new List<ContextTrack>();
        private FisherYates<ContextTrack> shuffle = new FisherYates<ContextTrack>();
        private volatile bool isPlayingQueue = false;
        private volatile bool cannotLoadMore = false;
        private volatile int shuffleKeepIndex = -1;

        public readonly PlayerState State;
        private readonly AbsSpotifyContext _context;
        private readonly LocalStateWrapper _stateWrapper;
        internal TracksKeeper(LocalStateWrapper stateWrapper,
            PlayerState state, 
            AbsSpotifyContext context)
        {
            _stateWrapper = stateWrapper;
            State = state;
            _context = context;
        }
        public bool IsPlayingFirst() => State.Index.Track == 0;
        public bool IsPlayingLast()
        {
            if (cannotLoadMore && !Queue.Any()) return State.Index.Track == Tracks.Count;
            return false;
        }
        public void InitializeFrom(Func<List<ContextTrack>, int> getIndex,
            ContextTrack track, Queue contextQueue)
        {
            Tracks.Clear();
            Queue.Clear();

            while (true)
            {
                if (_stateWrapper.Pages.NextPage())
                {
                    var newTrack = _stateWrapper.Pages.CurrentPage();
                    var index = getIndex(newTrack);
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

            if (contextQueue != null)
            {
                Queue.Add(contextQueue.Tracks.ToArray());
                isPlayingQueue = contextQueue.IsPlayingQueue;
                UpdateState();
            }

            CheckComplete();

            if (!PlayableId.CanPlaySomething(Tracks))
                throw new UnsupportedContextException("Cannot play anything");

            try
            {
                if (track != null) EnrichCurrentTrack(track);
            }
            catch (Exception x)
            {
                Debug.WriteLine($"Failed updating current track metadata, {x.ToString()}");
            }
        }
        private void EnrichCurrentTrack(ContextTrack track)
        {
            if (isPlayingQueue)
            {
                var builder = State.Track;
                ProtoUtils.EnrichTrack(builder, track);
            }
            else
            {
                int index = (int)State.Index.Track;
                var current = Tracks[index];
                ProtoUtils.EnrichTrack(current, track);
                Tracks[index] = current;
                State.Track = ProtoUtils.ConvertToProvidedTrack(current, State.ContextUri);
            }
        }
        public void UpdateContext([NotNull] List<ContextPage> updatedPages)
        {
            var updatedTracks = updatedPages
                .SelectMany(x => x.Tracks).ToList();
            foreach (var track in updatedTracks)
            {
                var index = Tracks.FindIndex(x => x.Uri == track.Uri);
                if (index == -1) continue;

                var b = new ContextTrack(Tracks[index]);
                ProtoUtils.CopyOverMetadata(track, b);
                Tracks[index] = b;

                if (index != State.Index.Track) continue;
                ProtoUtils.CopyOverMetadata(track, State.Track);
                UpdateLikeDislike();
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
                State.ContextMetadata.GetMetadataOrDefault("transforming.shuffle", "true"));
            if (_context.IsFinite() && _stateWrapper.IsShufflingContext
                                    && transformingShuffle) ShuffleEntirely();
            else State.Options.ShufflingContext = false; // Must do this directly!

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

            var currentlyPlaying = PlayableId.FromUri(State.Track.Uri);

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
                State.ContextMetadata.AddOrUpdate("track_count", (Tracks.Count + Queue.Count).ToString());
            else
                State.ContextMetadata.Remove("track_count");
        }

        public void CheckComplete()
        {
            if (cannotLoadMore) return;

            if (_context.IsFinite())
            {
                var totalTracks = int.Parse(State.ContextMetadata.GetMetadataOrDefault("track_count", "-1"));
                if (totalTracks == -1) cannotLoadMore = false;
                else cannotLoadMore = totalTracks == Tracks.Count;
            }
            else
                cannotLoadMore = false;
        }

        public void SetCurrentTrackIndex(int index)
        {
            if (isPlayingQueue) throw new IllegalStateException();
            State.Index = new ContextIndex
            {
                Track = (uint) index
            };
            UpdateState();
        }

        public void UpdateState()
        {
            if (isPlayingQueue)
            {
                var head = Queue.First;
                State.Track = ProtoUtils.ConvertToProvidedTrack(head.Value, State.ContextUri);
                Queue.Remove(head);
            }
            else
            {
                var itemAtIndex = Tracks[(int) State.Index.Track];
                State.Track = ProtoUtils.ConvertToProvidedTrack(itemAtIndex, State.ContextUri);
            }

            UpdateLikeDislike();
            UpdateTrackDuration();
            UpdatePrevNextTracks();
        }

        public void UpdateLikeDislike()
        {
            if (string.Equals(State.ContextMetadata.GetMetadataOrDefault("like-feedback-enabled", "0"), "1"))
            {
                State.ContextMetadata.AddOrUpdate("like-feedback-selected",
                    State.Track.Metadata.GetMetadataOrDefault("like-feedback-selected", "0"));
            }
            else
            {
                State.ContextMetadata.Remove("like-feedback-selected");
            }

            if (string.Equals(State.ContextMetadata.GetMetadataOrDefault("dislike-feedback-enabled", "0"), "1"))
            {
                State.ContextMetadata.AddOrUpdate("dislike-feedback-selected",
                    State.Track.Metadata.GetMetadataOrDefault("dislike-feedback-selected", "0"));
            }
            else
            {
                State.ContextMetadata.Remove("dislike-feedback-selected");
            }
        }

        public void UpdateTrackDuration()
        {
            var current = State.Track;
            State.Duration = current.Metadata.ContainsKey("duration")
                ? long.Parse(current.Metadata["duration"])
                : 0L;
        }

        public void UpdateTrackDuration(int duration)
        {
            State.Duration = duration;
            State.Track.Metadata["duration"] = duration.ToString();
            UpdateMetadataFor((int)State.Index.Track, "duration", duration.ToString());
        }
        public void UpdateMetadataFor(int index, [NotNull] string key, [NotNull] string value)
        {
            var ct = Tracks[index];
            ct.Metadata[key] = value;
            Tracks[index] = ct;
        }

        public void UpdatePrevNextTracks()
        {
            var index = (int)State.Index.Track;

            State.PrevTracks.Clear();
            for (int i = Math.Max(0, index - MAX_PREV_TRACKS); i < index; i++)
            {
                State.PrevTracks.Add(ProtoUtils.ConvertToProvidedTrack(Tracks[i], State.ContextUri));
            }


            State.NextTracks.Clear();
            for (int i = index + 1; i < Math.Min(Tracks.Count, index + 1 + MAX_PREV_TRACKS); i++)
            {
                State.NextTracks.Add(ProtoUtils.ConvertToProvidedTrack(Tracks[i], State.ContextUri));
            }
        }
    }
}
