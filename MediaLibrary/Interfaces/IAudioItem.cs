using System.Collections.Generic;
using MediaLibrary.Enums;

namespace MediaLibrary.Interfaces
{
    public interface IAudioItem
    {
        AudioServiceType AudioService { get; }
        IAudioId Id { get; }
        AudioItemType Type { get; }
        List<UrlImage> Images { get; }
        string Name { get; }
        string Description { get; }
    }
}
