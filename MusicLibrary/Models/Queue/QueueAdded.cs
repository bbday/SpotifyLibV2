using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;

namespace MusicLibrary.Models.Queue
{
    public class QueueAddedItem : IQueueUpdateItem
    {
        public QueueAddedItem(IAudioId id, 
            object metadata, 
            int insertedAt)
        {
            Id = id;
            Metadata = metadata;
            InsertedAt = insertedAt;
        }
        public QueueUpdateReason Reason => QueueUpdateReason.Added;
        public IAudioId Id { get; }
        public object Metadata { get; }

        public int InsertedAt { get; }
    }
}
