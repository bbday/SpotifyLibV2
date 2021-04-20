using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Connectstate;
using MediaLibrary.Interfaces;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLibrary.Audio.Context;
using SpotifyLibrary.Connect;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Audio
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

        private readonly AbsSpotifyContext _context;
        private readonly LocalStateWrapper _stateWrapper;

        internal TracksKeeper(LocalStateWrapper stateWrapper,
            AbsSpotifyContext context,
            IMercuryClient mercury)
        {
            _stateWrapper = stateWrapper;
            _context = context;
            Mercury = mercury;
        }

        public bool IsPlayingFirst() => _stateWrapper.PlayerState.Index.Track == 0;

        public bool IsPlayingLast()
        {
            if (cannotLoadMore && !Queue.Any()) return _stateWrapper.PlayerState.Index.Track == Tracks.Count;
            return false;
        }

        public void InitializeFrom(Func<List<ContextTrack>, int> getIndex,
            ContextTrack? track, Queue? contextQueue)
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

        public void UpdateTrackCount()
        {
            if (_context.IsFinite())
                _stateWrapper.PlayerState.ContextMetadata.AddOrUpdate("track_count",
                    (Tracks.Count + Queue.Count).ToString());
            else
                _stateWrapper.PlayerState.ContextMetadata.Remove("track_count");
        }

        public IMercuryClient Mercury { get; }

        public void ToggleShuffle(bool setTrue)
        {
            //throw new NotImplementedException();
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
                _stateWrapper.PlayerState.ContextMetadata.GetMetadataOrDefault("transforming.shuffle", "true"));
            if (_context.IsFinite() && _stateWrapper.IsShufflingContext
                                    && transformingShuffle) ShuffleEntirely();
            else _stateWrapper.PlayerState.Options.ShufflingContext = false; // Must do this directly!

            SetCurrentTrackIndex(0);
        }
        public void CheckComplete()
        {
            if (cannotLoadMore) return;

            if (_context.IsFinite())
            {
                var totalTracks =
                    int.Parse(_stateWrapper.PlayerState.ContextMetadata.GetMetadataOrDefault("track_count", "-1"));
                if (totalTracks == -1) cannotLoadMore = false;
                else cannotLoadMore = totalTracks == Tracks.Count;
            }
            else
                cannotLoadMore = false;
        }
        public void ShuffleEntirely()
        {

        }
        public void SetCurrentTrackIndex(int index)
        {
            if (isPlayingQueue) throw new IllegalStateException();
            _stateWrapper.PlayerState.Index = new ContextIndex
            {
                Track = (uint)index
            };
            UpdateState();
        }
        public void UpdateTrackDuration()
        {
            var current = _stateWrapper.PlayerState.Track;
            _stateWrapper.PlayerState.Duration = current.Metadata.ContainsKey("duration")
                ? long.Parse(current.Metadata["duration"])
                : 0L;
        }

        public void UpdateTrackDuration(int duration)
        {
            _stateWrapper.PlayerState.Duration = duration;
            _stateWrapper.PlayerState.Track.Metadata["duration"] = duration.ToString();
            UpdateMetadataFor((int)_stateWrapper.PlayerState.Index.Track, "duration", duration.ToString());
        }
        public void UpdateMetadataFor(int index,  string key,string value)
        {
            var ct = Tracks[index];
            ct.Metadata[key] = value;
            Tracks[index] = ct;
        }

        public void UpdateState()
        {
            if (isPlayingQueue)
            {
                var head = Queue.First;
                _stateWrapper.PlayerState.Track =
                    ProtoUtils.ConvertToProvidedTrack(head.Value, _stateWrapper.PlayerState.ContextUri);
                Queue.Remove(head);
            }
            else
            {
                var itemAtIndex = Tracks[(int)_stateWrapper.PlayerState.Index.Track];
                _stateWrapper.PlayerState.Track =
                    ProtoUtils.ConvertToProvidedTrack(itemAtIndex, _stateWrapper.PlayerState.ContextUri);
            }

            UpdateTrackDuration();
        }
        private void EnrichCurrentTrack(ContextTrack track)
        {
            if (isPlayingQueue)
            {
                var builder = _stateWrapper.PlayerState.Track;
                ProtoUtils.EnrichTrack(builder, track);
            }
            else
            {
                int index = (int)_stateWrapper.PlayerState.Index.Track;
                var current = Tracks[index];
                ProtoUtils.EnrichTrack(current, track);
                Tracks[index] = current;
                _stateWrapper.PlayerState.Track =
                    ProtoUtils.ConvertToProvidedTrack(current, _stateWrapper.PlayerState.ContextUri);
            }
        }
    }
}