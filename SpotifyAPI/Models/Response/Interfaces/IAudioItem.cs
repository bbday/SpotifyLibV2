using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response.Interfaces
{
    public interface IAudioItem
    {
        IAudioId Id { get; }
        AudioType Type { get; }
        List<UrlImage> Images { get; }
        string Name { get; }
        string Description { get; }
        string Href { get; }
    }
}
