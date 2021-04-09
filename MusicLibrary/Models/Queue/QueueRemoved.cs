using System;
using System.Collections.Generic;
using System.Text;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;

namespace MusicLibrary.Models.Queue
{
    public class QueueRemovedItem : IQueueUpdateItem
    {
        public QueueRemovedItem(IAudioId id,
            object metadata,
            int removedAt)
        {
            Id = id;
            Metadata = metadata;
            RemovedAt = removedAt;
        }

        public QueueUpdateReason Reason => QueueUpdateReason.Removed;
        public IAudioId Id { get; }
        public object Metadata { get; }
        public int RemovedAt { get; }
    }
}
