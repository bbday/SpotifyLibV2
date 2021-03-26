using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibrary.Connect.Player;

namespace SpotifyLibrary.Connect.PlayerSession
{
    public class PlayerQueue : IDisposable
    {
        public QueueNode<PlayerQueueEntry>? Head
        {
            get;
            private set;
        } = null;

        public QueueNode<PlayerQueueEntry>? Next => Head?.Next;

        public async Task Add(PlayerQueueEntry entry)
        {
            if (Head == null) Head = new QueueNode<PlayerQueueEntry>(entry,
                default,
                default);
            else Head.Next = new QueueNode<PlayerQueueEntry>(entry);

            await entry.Start();
            Debug.WriteLine($"Added to queue {entry}");
        }

        public void Swap(QueueNode<PlayerQueueEntry> oldEntry,
            PlayerQueueEntry newEntry)
        {
            if (Head == null) return;

            var swapped = false;
            if (Head == oldEntry)
            {
                Head = new QueueNode<PlayerQueueEntry>(newEntry,
                    oldEntry.Next.Item,
                    oldEntry.Previous.Item);
            }
            else
            {
                swapped = Head.Swap(oldEntry, newEntry);
            }

            oldEntry?.Dispose();
            if (swapped)
            {
                Debug.WriteLine($"{oldEntry.Item.Id} swapped with {newEntry.Id}");
            }
        }

        public void Remove(PlayerQueueEntry entry)
        {
            if (Head == null) return;

            bool removed = false;
            if (Head.Item.Equals(entry))
            {
                var tmp = Head;
                Head = tmp.Next;
                tmp.Dispose();
                removed = true;
            }
            else
            {
                removed = Head.Remove(entry);
            }
            if (removed) Debug.WriteLine("{0} removed from queue.", entry);
        }
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
    }
}
