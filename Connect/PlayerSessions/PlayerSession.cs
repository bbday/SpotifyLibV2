#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Connect.Transitions;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Connect.PlayerSessions
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
    public class PlayerQueueEntry 
    {
        private ISpotifyPlayer _player;
        private SpotifyConfiguration conf;
        private bool preloaded;
        private PlayerSession playerSession;
        private int _initialSeek;
        public PlayerQueueEntry(ISpotifyPlayer player, 
            SpotifyConfiguration conf,
            IPlayableId playable, 
            bool preloaded, 
            int initialSeek,
            PlayerSession playerSession)
        {
            this._player = player;
            this.conf = conf;
            _initialSeek = initialSeek;
            this.Id = playable;
            this.preloaded = preloaded;
            this.playerSession = playerSession;
            PlaybackId = LocalStateWrapper.GeneratePlaybackId();

        }

        public Task Play() => _player.Load(Id, preloaded, _initialSeek);

        public bool Equals(PlayerQueueEntry other)
        {
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerQueueEntry other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }


        public IPlayableId Id { get; }
        public string PlaybackId { get; }

        public Task Seek(int pos)
        {
            //TODO
            return Task.CompletedTask;
        }
    }
    public class PlayerSession
    {
        public string SessionId { get; }
        private readonly SpotifyConfiguration _conf;
        private readonly ISpotifyPlayer _player;
        public PlayerSession(ISpotifyPlayer player, 
            SpotifyConfiguration conf,
            string sessionId)
        {
            _player = player;
            _conf = conf;
            _queue = new PlayerQueue();
            SessionId = sessionId;
            
            Debug.WriteLine($"Created new session: {sessionId}");

            _ = player.ClearOutput();
        }
        private readonly PlayerQueue _queue; 
        private int LastPlayPos = 0;
        private TransitionReason? LastPlayReason = null;
        public async Task<string> Play(IPlayableId stateWrapperGetPlayableItem, double stateWrapperPosition, TransitionReason transitioninfoStartedReason)
        {
            return (await PlayInternal(stateWrapperGetPlayableItem, stateWrapperPosition, transitioninfoStartedReason)
                ).entry.Item.PlaybackId;
        }

        public async Task<(QueueNode<PlayerQueueEntry> entry, int Position)> PlayInternal(IPlayableId stateWrapperGetPlayableItem, double stateWrapperPosition, TransitionReason transitioninfoStartedReason)
        {
            LastPlayPos = (int)stateWrapperPosition;
            LastPlayReason = transitioninfoStartedReason;

            if (!AdvanceTo(stateWrapperGetPlayableItem))
            {
                Add(stateWrapperGetPlayableItem, false);
                _queue.Advance();
            }

            var head = _queue.Head;
            if (head == null)
                throw new IllegalStateException();
            await head.Item.Play();
            var customFade = false;
            if (head.Previous != null)
            {
                //TODO Crossfade
            }

            //TODO
            head.Item.Seek(LastPlayPos);
            Debug.WriteLine($"{head.Item.Id} has been added to output.");
            return (head, LastPlayPos);
        }
        private void Add([NotNull] IPlayableId playable, bool preloaded)
        {
            var entry = new PlayerQueueEntry(_player, _conf, playable, preloaded, LastPlayPos,
                this);
            _queue.Add(entry);
        }
        private bool AdvanceTo(IPlayableId id)
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
    }
}
