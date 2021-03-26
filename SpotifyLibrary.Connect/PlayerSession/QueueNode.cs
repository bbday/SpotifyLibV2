using System;
using JetBrains.Annotations;
using SpotifyLibrary.Connect.PlayerSession;

namespace SpotifyLibrary.Connect.Player
{
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

        public virtual bool DisposeIfUseless()
        {
            bool isUseless = true;
            if (isUseless) Dispose();
            return isUseless;
        }

        public QueueNode(T item,
            [CanBeNull] T? next = default,
            T? previous = default)
        {
            Item = item;
            if (next != null)
            {
                Next = new QueueNode<T>(next);
            }

            if (previous != null)
            {
                Previous = new QueueNode<T>(previous);
            }
        }

        public T Item { get; }
        public QueueNode<T>? Next { get; set; }
        public QueueNode<T>? Previous { get; set; }

        public static bool operator ==(QueueNode<T>? obj1, QueueNode<T>? obj2)
        {
            if (obj1 is null && obj2 is null) return true;
            if (obj1 is null && !(obj2 is null)) return false;
            if (!(obj1 is null) && (obj2 is null)) return false;

            return obj1.Item.Equals(obj2.Item);
        }

        public static bool operator !=(QueueNode<T>? obj1, QueueNode<T> obj2)
        {
            if (obj1 is null && obj2 is null) return false;
            if (obj1 is null && !(obj2 is null)) return true;
            if (!(obj1 is null) && (obj2 is null)) return true;

            return !obj1.Item.Equals(obj2.Item);
        }

        public void SetNext([NotNull] PlayerQueueEntry entry)
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

        public bool Swap([NotNull] QueueNode<T> oldEntry,
            [NotNull] T newEntry)
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

        public bool Remove([NotNull] T entry)
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