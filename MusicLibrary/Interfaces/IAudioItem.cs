using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Enum;
using MusicLibrary.Models;

namespace MusicLibrary.Interfaces
{
    public interface IAudioItem
    {
        AudioService AudioService { get; }
        IAudioId Id { get; }
        AudioType Type { get; }
        List<UrlImage> Images { get;}
        string Name { get; }
        string Description { get; }
    }
}
