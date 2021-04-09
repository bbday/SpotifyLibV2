using MusicLibrary.Enum;

namespace MusicLibrary.Interfaces
{
    public interface IQueueUpdateItem
    {
        QueueUpdateReason Reason { get; }
        IAudioId Id { get; }
        object Metadata { get; }
    }
}
