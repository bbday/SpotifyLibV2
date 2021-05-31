using System.Diagnostics;
using Spotify.Lib.Connect.Audio;
using Spotify.Lib.Exceptions;
using Spotify.Lib.Interfaces;
using static Spotify.Lib.Connect.DataHolders.PlayerQueueHelper;
namespace Spotify.Lib.Connect.DataHolders
{
    internal static class SessionHelper
    {
        internal static (QueueNode<AbsChunkedStream>? head, int lastPlayPos) LoadSession(ref PlayerSessionHolder sessionHolder,
            ISpotifyPlayer player,
            AbsChunkedStream stream,
            int pos, TransitionReason startedReason)
        {
            var sessionHolderQueue = sessionHolder.Queue.Value;

            var lastPlayPos = (int)pos;
            var lastPlayReason = startedReason;

            if (!AdvanceTo(ref sessionHolder, stream))
            {
                Add(stream, player, ref sessionHolderQueue);
                Advance(ref sessionHolderQueue);
            }
            var head = sessionHolderQueue.Head;
            if (head == null)
                throw new IllegalStateException();

            Debug.WriteLine($"{head.Item.Id} has been added to output.");
            sessionHolder.Queue = sessionHolderQueue;
            return (head, lastPlayPos);
        }
        internal static bool AdvanceTo(ref PlayerSessionHolder session,
            AbsChunkedStream id)
        {
            var queue = session.Queue.Value;

            do
            {
                var entryTemp = queue.Head;
                if (entryTemp == null) return false;
                var entryValue = entryTemp;
                if (entryValue.Item.Equals(id))
                {
                    var next = entryValue.Next;
                    if (next == null || !next.Item.Equals(id))
                    {
                        return true;
                    }
                }

                session.Queue = queue;
            } while (Advance(ref queue));
            session.Queue = queue;

            return false;
        }

    }
    internal struct PlayerSessionHolder
    {
        internal PlayerSessionHolder(
            string sessionId,
            IPlayerSessionListener sessionListener)
        {
            SessionId = sessionId;
            Listener = sessionListener;
            Queue = new PlayerQueue();
        }
        public PlayerQueue? Queue;
        public readonly string SessionId;
        public readonly IPlayerSessionListener Listener;
    }
}
