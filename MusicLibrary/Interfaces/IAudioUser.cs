using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Models;

namespace MusicLibrary.Interfaces
{
    public interface IAudioUser
    {
        IAudioId Id { get; }
        AudioType Type { get; }
        AudioService Service { get; }
        string DisplayName { get; }
        List<UrlImage> Images { get; }
    }
}
