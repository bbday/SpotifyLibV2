using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Connectstate;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Spotify.Lib.Connect.Contexts;
using Spotify.Lib.Exceptions;
using Spotify.Lib.Helpers;
using Spotify.Lib.Models;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using static Spotify.Lib.Connect.DataHolders.PagesHelper;
namespace Spotify.Lib.Connect.DataHolders
{
    internal struct TracksKeeper
    {
        internal bool IsPlayingQueue;
        internal bool CannotLoadMore;

        internal readonly List<ContextTrack> Tracks;
        internal readonly LinkedList<ContextTrack> Queue;
        internal readonly AbsSpotifyContext Context;

        internal TracksKeeper(
            AbsSpotifyContext context)
        {
            Context = context;
            IsPlayingQueue = false;
            CannotLoadMore = false;

            Queue = new LinkedList<ContextTrack>();
            Tracks = new List<ContextTrack>();
        }
    }

    internal static class TracksKeeperHelpers
    {
        public static void ToggleShuffle(TracksKeeper? keeper, bool setTrue)
        {
            //throw new NotImplementedException();
        }
        internal static void EnrichCurrentTrack(
            ref TracksKeeper keeper, 
            PlayerState state,
            ContextTrack track)
        {
            if (keeper.IsPlayingQueue)
            {
                var builder = state.Track;
                ProtoUtils.EnrichTrack(builder, track);
            }
            else
            {
                int index = (int)state.Index.Track;
                var current = keeper.Tracks[index];
                ProtoUtils.EnrichTrack(current, track);
                keeper.Tracks[index] = current;
                state.Track =
                    ProtoUtils.ConvertToProvidedTrack(current, state.ContextUri);

            }
        }

        internal static void InitializeStart(ref TracksKeeper tracksKeeper,
            AbsSpotifyContext context,
            PlayerState state,
            ref Pages pages)
        {
            if (!tracksKeeper.CannotLoadMore)
            {
                if (!NextPage(ref pages)) 
                    throw new IllegalStateException();

                tracksKeeper.Tracks.Clear();
                tracksKeeper.Tracks.AddRange(pages.CurrentPage);
            }

            CheckComplete(ref tracksKeeper, context, state);
            if (!PlayableId.CanPlaySomething(tracksKeeper.Tracks))
                throw new UnsupportedContextException("cannot play anything");

            var transformingShuffle = bool.Parse(
                state.ContextMetadata.GetMetadataOrDefault("transforming.shuffle", "true"));
            if (context.IsFinite() && SpotifyRequestListener.IsShufflingContext(state)
                                    && transformingShuffle) ShuffleEntirely();
            else state.Options.ShufflingContext = false; // Must do this directly!

            SetCurrentTrackIndex(ref tracksKeeper, state, 0);
        }
        public static void ShuffleEntirely()
        {
            //TODO
        }
        internal static void InitializeFrom(
            ref TracksKeeper keeper,
            ref Pages pages,
            PlayerState state,
            AbsSpotifyContext context,
            Func<List<ContextTrack>, int> getIndex,
            ContextTrack? track,
            Queue queue)
        {
            keeper.Tracks.Clear();
            keeper.Queue.Clear();
            while (true)
            {
                if (NextPage(ref pages))
                {
                    var tracks = pages.CurrentPage;
                    var index = getIndex(tracks);
                    if (index == -1)
                    {
                        Debug.WriteLine($"Could not find track, going to next page.");
                        keeper.Tracks.AddRange(tracks);
                        continue;
                    }

                    index += keeper.Tracks.Count;
                    keeper.Tracks.AddRange(tracks);
                    
                    SetCurrentTrackIndex(ref keeper, state, index);
                    Debug.WriteLine($"Initialized current track index to {index}");
                    break;
                }
                else
                {
                    keeper.CannotLoadMore = true;
                    UpdateTrackCount(ref keeper, context, state);
                    throw new IllegalStateException("Couldn't find current track!");
                }
            }

            if (queue != null)
            {
                keeper.Queue.Add(queue.Tracks.ToArray());
                keeper.IsPlayingQueue = queue.IsPlayingQueue;
                UpdateState(ref keeper, state);
            }

            CheckComplete(ref keeper, context, state);

            if (!PlayableId.CanPlaySomething(keeper.Tracks))
                throw new UnsupportedContextException("Cannot play anything");

            try
            {
                if (track != null) EnrichCurrentTrack(ref keeper, state, track);
            }
            catch (Exception x)
            {
                Debug.WriteLine($"Failed updating current track metadata, {x.ToString()}");
            }

        }
        public static void CheckComplete(
            ref TracksKeeper keeper,
            AbsSpotifyContext context,
            PlayerState state)
        {
            if (keeper.CannotLoadMore) return;

            if (context.IsFinite())
            {
                var totalTracks =
                    int.Parse(state.ContextMetadata.GetMetadataOrDefault("track_count", "-1"));
                if (totalTracks == -1)
                {
                    keeper.CannotLoadMore = false;
                }
                else
                {
                    keeper.CannotLoadMore = totalTracks == keeper.Tracks.Count;
                }
            }
            else
                keeper.CannotLoadMore = false;
        }
        public static void SetCurrentTrackIndex(ref TracksKeeper keeper,
            PlayerState state,
            int index)
        {
            if (keeper.IsPlayingQueue) throw new IllegalStateException();
            state.Index = new ContextIndex
            {
                Track = (uint)index
            };
            UpdateState(ref keeper, state);
        }
        public static void UpdateState(
            ref TracksKeeper keeper,
            PlayerState state)
        {
            if (keeper.IsPlayingQueue)
            {
                var head = keeper.Queue.First;
                state.Track =
                    ProtoUtils.ConvertToProvidedTrack(head.Value, state.ContextUri);
                keeper.Queue.Remove(head);
            }
            else
            {
                var itemAtIndex = keeper.Tracks[(int)state.Index.Track];
                state.Track =
                    ProtoUtils.ConvertToProvidedTrack(itemAtIndex, state.ContextUri);
            }

            UpdateTrackDuration(state);
            UpdatePrevNextTracks(state, keeper);
        }
        private static readonly int MAX_PREV_TRACKS = 16;
        private static readonly int MAX_NEXT_TRACKS = 48;
        private static void UpdatePrevNextTracks(PlayerState state,
            TracksKeeper keeper)
        {
            var index = (int)state.Index.Track;

            state.PrevTracks.Clear();
            for (var i = Math.Max(0, index - MAX_PREV_TRACKS); i < index; i++)
                state.PrevTracks.Add(ProtoUtils.ToProvidedTrack(keeper.Tracks[i],
                    state.ContextUri));

            state.NextTracks.Clear();
            state.NextTracks.AddRange(keeper.Queue.Select(z => ProtoUtils.ToProvidedTrack(z, state.ContextUri)));

            for (var i = index + 1; i < Math.Min(keeper.Tracks.Count, index + 1 + MAX_NEXT_TRACKS); i++)
                state.NextTracks.Add(ProtoUtils.ToProvidedTrack(keeper.Tracks[i], state.ContextUri));
        }
        public static void UpdateTrackCount(ref TracksKeeper keeper,
            AbsSpotifyContext context,
            PlayerState state)
        {
            if (context.IsFinite())
                state.ContextMetadata.AddOrUpdate("track_count",
                    (keeper.Tracks.Count + keeper.Queue.Count).ToString());
            else
                state.ContextMetadata.Remove("track_count");
        }
        public static void UpdateTrackDuration(
            PlayerState state)
        {
            var current = state.Track;
            state.Duration = current.Metadata.ContainsKey("duration")
                ? long.Parse(current.Metadata["duration"])
                : 0L;
        }
        public static void UpdateTrackDuration(ref TracksKeeper trackskeeper, 
            PlayerState state,
            int duration)
        {
            state.Duration = duration;
            state.Track.Metadata["duration"] = duration.ToString();
            UpdateMetadataFor(ref trackskeeper,
                (int)state.Index.Track, "duration", duration.ToString());
        }
        public static void UpdateMetadataFor(
            ref TracksKeeper trackkeeper,
            int index, string key, string value)
        {
            var builder = trackkeeper.Tracks[index];
            builder.Metadata[key] =  value;
            trackkeeper.Tracks[index] = builder;
        }
    }
}
