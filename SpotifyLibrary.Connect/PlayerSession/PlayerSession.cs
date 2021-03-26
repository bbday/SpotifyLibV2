using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Connect.Exceptions;
using SpotifyLibrary.Connect.Player;
using SpotifyLibrary.Connect.Transitions;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Player;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Connect.PlayerSession
{
    public interface IPlayerSessionListener
    {
        ISpotifyId CurrentPlayable();
        ISpotifyId NextPlayable();
        ISpotifyId NextPlayableDoNotSet();
        Dictionary<string, string> MetadataFor(ISpotifyId playable);
        void PlaybackHalted(int chunk);
        void PlaybackResumedFromHalt(int chunk, long diff);
        void StartedLoading();
        void LoadingError(Exception ex);
        void FinishedLoading(TrackOrEpisode metadata);
        void PlaybackError(Exception ex);

        void TrackChanged(string playbackId, TrackOrEpisode metadata, int pos, TransitionReason startedReason);
        void TrackPlayed(string playbackId, TransitionReason endReason, int endedAt);
    }

    public interface IPlayerQueueListener
    {

        void PlaybackError(PlayerQueueEntry entry, Exception ex);


        void PlaybackEnded(PlayerQueueEntry entry);


        void PlaybackHalted(PlayerQueueEntry entry, int chunk);


        void PlaybackResumed(PlayerQueueEntry entry, int chunk, int diff);


        void InstantReached(PlayerQueueEntry entry, int callbackId, int exactTime);


        void StartedLoading(PlayerQueueEntry entry);

        void LoadingError(PlayerQueueEntry entry, Exception ex, bool retried);


        void FinishedLoading(PlayerQueueEntry entry, TrackOrEpisode metadata);

        Dictionary<string, string> MetadataFor(ISpotifyId playable);
    }
    public class PlayerSession : IPlayerQueueListener
    {

        private readonly IPlayerSessionListener _listener;
        private int LastPlayPos = 0;
        private TransitionReason? LastPlayReason = null;
        private readonly ISpotifyPlayer _player;
        private readonly string _sessionId;
        private readonly SpotifyConfiguration _config;
        public PlayerSession(ISpotifyPlayer player,
            SpotifyConfiguration config,
            string sessionId,
            IPlayerSessionListener listener,
            ICdnManager cdnManager)
        {

            _player = player;
            _config = config;
            _sessionId = sessionId;
            _listener = listener;
            _cdnManager = cdnManager;
            _queue = new PlayerQueue();
        }
        private readonly PlayerQueue _queue;
        private readonly ICdnManager _cdnManager;

        public async Task<string> Load(ISpotifyId stateWrapperGetPlayableItem,
            int stateWrapperPosition,
            TransitionReason transitionInfoStartedReason)
        {
            return (await LoadInternal(stateWrapperGetPlayableItem,
                stateWrapperPosition, transitionInfoStartedReason)).Entry.Item.PlaybackId;
        }

        private async Task<(QueueNode<PlayerQueueEntry> Entry, int Position)> LoadInternal(ISpotifyId stateWrapperGetPlayableItem,
            int stateWrapperPosition, TransitionReason transitionInfoStartedReason)
        {
            LastPlayPos = (int)stateWrapperPosition;
            LastPlayReason = transitionInfoStartedReason;

            if (!AdvanceTo(stateWrapperGetPlayableItem))
            {
                await Add(stateWrapperGetPlayableItem, false);
                _queue.Advance();
            }
            var head = _queue.Head;
            if (head == null)
                throw new IllegalStateException();

            Debug.WriteLine($"{head.Item.Id} has been added to output.");
            return (head, LastPlayPos);
        }
        private Task Add([NotNull] ISpotifyId playable, bool preloaded)
        {
            var entry = new PlayerQueueEntry(_player, playable, this, _cdnManager);
            return _queue.Add(entry);
        }
        private bool AdvanceTo(ISpotifyId id)
        {
            do
            {
                var entry = _queue.Head;
                if (entry == null) return false;
                if (entry.Item.Id.Equals(id))
                {
                    var next = _queue.Next;
                    if (next == null || !next.Item.Id.Equals(id))
                    {
                        return true;
                    }
                }
            } while (_queue.Advance());

            return false;
        }


        public void PlaybackError(PlayerQueueEntry entry, Exception ex)
        {
            if (entry.Equals(_queue.Head!.Item)) _listener.PlaybackError(ex);
            _queue.Remove(entry);
        }

        public async void PlaybackEnded(PlayerQueueEntry entry)
        {
            _listener.TrackPlayed(entry.PlaybackId, entry.EndReason, -1);

            if (entry.Equals(_queue.Head!.Item))
                await Advance(entry.EndReason);
        }

        public void PlaybackHalted(PlayerQueueEntry entry, int chunk)
        {
            if (entry.Equals(_queue.Head!.Item)) _listener.PlaybackHalted(chunk);
        }

        public void PlaybackResumed(PlayerQueueEntry entry, int chunk, int diff)
        {
            if (entry.Equals(_queue.Head!.Item)) _listener.PlaybackResumedFromHalt(chunk, diff);
        }

        public void InstantReached(PlayerQueueEntry entry, int callbackId, int exactTime)
        {
            throw new NotImplementedException();
        }

        public void StartedLoading(PlayerQueueEntry entry)
        {
            Debug.WriteLine("{0} started loading.", entry);
            if (entry.Equals(_queue.Head!.Item)) _listener.StartedLoading();
        }

        public void LoadingError(PlayerQueueEntry entry, Exception ex, bool retried)
        {
            throw new NotImplementedException();
        }

        public void FinishedLoading(PlayerQueueEntry entry, TrackOrEpisode metadata)
        {
            Debug.WriteLine("{0} finished loading.", entry);
            if (entry.Equals(_queue.Head!.Item)) _listener.FinishedLoading(metadata);
        }

        public Dictionary<string, string> MetadataFor(ISpotifyId playable) => _listener.MetadataFor(playable);

        private async Task Advance(TransitionReason reason)
        {
            var next = _listener.NextPlayable();
            if (next == null)
                return;

            var entry = await LoadInternal(next,
                0, reason);
            _listener.TrackChanged(entry.Entry.Item.PlaybackId, entry.Entry.Item.Metadata, entry.Position, reason);
        }
    }
}
