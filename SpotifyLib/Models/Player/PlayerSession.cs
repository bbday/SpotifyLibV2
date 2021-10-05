using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SpotifyLib.Models.Transitions;

namespace SpotifyLib.Models.Player
{
    public class PlayerSession
    {
        private readonly IAudioOutput _player;
        private readonly string _sessionId;
        private readonly PlayerQueue _queue;
        public PlayerSession(IAudioOutput player, 
            string sessionId)
        {

            _player = player;
            _sessionId = sessionId;
            _queue = new PlayerQueue();
        }
        private int LastPlayPos = 0;
        private TransitionReason? LastPlayReason = null;

        public async Task<ChunkedStream> Load(SpotifyId stateWrapperGetPlayableItem,
            int stateWrapperPosition,
            TransitionReason transitionInfoStartedReason)
        {
            return (await LoadInternal(stateWrapperGetPlayableItem,
                stateWrapperPosition, transitionInfoStartedReason)).Entry.Item;
        }

        private async Task<(QueueNode<ChunkedStream> Entry, int Position)>
            LoadInternal(
            SpotifyId stateWrapperGetPlayableItem,
            int stateWrapperPosition, TransitionReason transitionInfoStartedReason)
        {
            LastPlayPos = (int)stateWrapperPosition;
            LastPlayReason = transitionInfoStartedReason;

            if (!AdvanceTo(stateWrapperGetPlayableItem))
            {
                await Add(stateWrapperGetPlayableItem);
                PlayerQueueHelper.Advance(_queue);
            }
            var head = _queue.Head;
            if (head == null)
                throw new IllegalStateException();

            Debug.WriteLine($"{head.Item} has been added to output.");
            return (head, LastPlayPos);
        }
        private Task Add(SpotifyId playable)
        {
            //TODO: fetch stream..
            var str = new ChunkedStream(playable, "docu.m4a"); 
            PlayerQueueHelper.Add(str, _player, _queue);
            return Task.CompletedTask;
        }
        private bool AdvanceTo(SpotifyId id)
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
            } while (PlayerQueueHelper.Advance(_queue));

            return false;
        }
    }
}
