using MusicLibrary.Enum;
using MusicLibrary.Interfaces;

namespace MusicLibrary.Models.Queue
{
    public class QueueMovedItem : IQueueUpdateItem
    {
        public QueueMovedItem(IAudioId id,
            object metadata,
            int from,
            int to)
        {
            Id = id;
            Metadata = metadata;
            From = from;
            To = to;
        }
        public QueueUpdateReason Reason => QueueUpdateReason.Moved;
        public IAudioId Id { get; }
        public object Metadata { get; }

        public int From { get; }
        public int To { get; }
    }
}