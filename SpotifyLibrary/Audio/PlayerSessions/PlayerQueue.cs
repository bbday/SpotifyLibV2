using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Audio.PlayerSessions
{
    internal class PlayerQueue : IDisposable
    {
        private readonly ISpotifyPlayer _lib;
        internal PlayerQueue(ISpotifyPlayer lib)
        {
            _lib = lib;
        }
        public QueueNode<AbsChunkedStream>? Head
        {
            get;
            private set;
        } = null;

        public QueueNode<AbsChunkedStream>? Next => Head?.Next;

        public bool Advance()
        {
            if (Head == null || Head.Next == null)
                return false;

            var tmp = Head.Next;
            Head.Next = null;
            Head.Previous = null;
            if (!Head.DisposeIfUseless()) tmp.Previous = Head;
            Head = tmp;
            return true;
        }

        public void Dispose()
        {
            if (Head != null) Head.Dispose();
        }

        public async Task Add(AbsChunkedStream entry)
        {
            if (Head == null) Head = new QueueNode<AbsChunkedStream>(entry,
                default,
                default);
            else Head.Next = new QueueNode<AbsChunkedStream>(entry);

            //Play item on player.
            await _lib.IncomingStream(entry);
            Debug.WriteLine($"Added to queue {entry}");
        }
    }
}
