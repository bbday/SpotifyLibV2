using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class EmptyItem : ISpotifyItem
    {
        public AudioService AudioService { get; }
        public IAudioId Id { get; }
        public AudioType Type { get; }
        public List<UrlImage> Images { get; }
        public string Name { get; }
        public string Description { get; }
        public string Href { get; }
        public string Uri { get; }
    }
}
