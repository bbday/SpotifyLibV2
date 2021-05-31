using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Spotify.Lib.Connect.Audio;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Connect.DataHolders
{
    internal static class PlayerQueueHelper
    {
        public static bool Advance(ref PlayerQueue queue)
        {
            if (queue.Head == null || queue.Head.Next == null)
                return false;

            var tmp = queue.Head.Next;
            tmp.Next = null;
            tmp.Previous = null;
            queue.Head = tmp;

            if (!queue.Head.DisposeIfUseless()) tmp.Previous = queue.Head;
            queue.Head = tmp;
            return true;
        }
        public static void Add(AbsChunkedStream entry, 
            ISpotifyPlayer player,
            ref PlayerQueue queue)
        {
            var head = queue.Head;
            if (head == null) 
            {
                head = new QueueNode<AbsChunkedStream>(entry,
                default,
                default);

            }
            else
            {
                var headTemp = head;
                headTemp.Next = new QueueNode<AbsChunkedStream>(entry);
                head = headTemp;
            }

            queue.Head = head;
            //Play item on player.
            _ = player.IncomingStream(entry);
            Debug.WriteLine($"Added to queue {entry}");
        }
    }
    public struct PlayerQueue
    {
        public QueueNode<AbsChunkedStream>? Head
        {
            get; set;
        }
        public QueueNode<AbsChunkedStream>? Next => Head?.Next;
    }
    public class QueueNode<T> : IDisposable
    {
        public bool Equals(QueueNode<T> other)
        {
            return Item.Equals(other.Item);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is QueueNode<T> other)) return false;
            return Item.Equals(other.Item);
        }

        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        public void Dispose()
        {
            Next?.Dispose();
            Previous?.Dispose();
        }

        public bool DisposeIfUseless()
        {
            bool isUseless = true;
            if (isUseless) Dispose();
            return isUseless;
        }

        public QueueNode(T item,
             T? next = default,
            T? previous = default)
        {
            Item = item;
            if (next != null)
            {
                Next = new QueueNode<T>(next);
            }
            else
            {
                Next = null;
            }

            if (previous != null)
            {
                Previous = new QueueNode<T>(previous);
            }
            else
            {
                Previous = null;
            }
        }

        public T Item { get; }
        public QueueNode<T>? Next { get; set; }
        public QueueNode<T>? Previous { get; set; }

        public static bool operator ==(QueueNode<T>? obj1, QueueNode<T>? obj2)
        {
            if (obj1 is null && obj2 is null) return true;
            if (obj1 is null && obj2 is not null) return false;
            if (obj1 is not null && obj2 is null) return false;

            return obj1.Item.Equals(obj2.Item);
        }

        public static bool operator !=(QueueNode<T>? obj1, QueueNode<T>? obj2)
        {
            return !(obj1 == obj2);
        }

        public void SetNext(T entry)
        {
            var newItem = new QueueNode<T>(Item);
            if (Next == null)
            {
                Next = newItem;
                newItem.Previous = this;
            }
            else
            {
                Next.SetNext(entry);
            }
        }

        public bool Swap(QueueNode<T> oldEntry, T newEntry)
        {
            if (Next == null) return false;
            if (Next == oldEntry)
            {
                Next = new QueueNode<T>(newEntry,
                    oldEntry.Previous.Item,
                    oldEntry.Next.Item);
                return true;
            }
            else
            {
                return Next.Swap(oldEntry, newEntry);
            }
        }

        public bool Remove(T entry)
        {
            if (Next == null) return false;
            if (Next.Item.Equals(entry))
            {
                var tmp = Next;
                Next = tmp.Next;
                tmp.Dispose();
                return true;
            }
            else
            {
                return Next.Remove(entry);
            }
        }

    }
}
