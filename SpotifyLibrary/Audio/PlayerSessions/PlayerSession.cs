using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibrary.Connect.Transitions;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Audio.PlayerSessions
{
    internal class PlayerSession
    {
        private int LastPlayPos = 0;
        private TransitionReason? LastPlayReason = null;

        private readonly PlayerQueue _queue;
        private readonly string _sessionId;
        private readonly ISpotifyLibrary _library;
        private readonly IPlayerSessionListener _listener;
        internal PlayerSession(ISpotifyLibrary library,
            ISpotifyPlayer player,
            string sessionId,
            IPlayerSessionListener sessionListener)
        {
            _library = library;
            _sessionId = sessionId;  
            _listener = sessionListener;
            _queue = new PlayerQueue(player);
        }

        public async Task<string> Load(AbsChunkedStream stream,
            int stateWrapperPosition,
            TransitionReason transitionInfoStartedReason)
        {
            return (await LoadInternal(stream,
                stateWrapperPosition, transitionInfoStartedReason)).Entry.Item.PlaybackId;
        }
        private async Task<(QueueNode<AbsChunkedStream> Entry, int Position)> LoadInternal(
            AbsChunkedStream stream,
            int stateWrapperPosition, TransitionReason transitionInfoStartedReason)
        {
            LastPlayPos = (int)stateWrapperPosition;
            LastPlayReason = transitionInfoStartedReason;

            if (!AdvanceTo(stream))
            {
                await Add(stream, false);
                _queue.Advance();
            }
            var head = _queue.Head;
            if (head == null)
                throw new IllegalStateException();

            Debug.WriteLine($"{head.Item.Id} has been added to output.");
            return (head, LastPlayPos);
        }

        private bool AdvanceTo(AbsChunkedStream id)
        {
            do
            {
                var entry = _queue.Head;
                if (entry == null) return false;
                if (entry.Item.Equals(id))
                {
                    var next = _queue.Next;
                    if (next == null || !next.Item.Equals(id))
                    {
                        return true;
                    }
                }
            } while (_queue.Advance());

            return false;
        }

        private Task Add(AbsChunkedStream playable, bool preloaded) => _queue.Add(playable);
    }
}
