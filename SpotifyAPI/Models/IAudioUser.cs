using System.Collections.Generic;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models
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
