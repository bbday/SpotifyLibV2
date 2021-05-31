using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct EmptyItem : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type { get; }
        public ISpotifyId Id { get; }
        public string Name { get; set; }
        public string Description { get; }
        public string Caption { get; }
        public List<UrlImage> Images { get; set; }
    }

    public struct SimpleArtist : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Artist;
        public ISpotifyId Id => new ArtistId(Uri);
        public string Name { get; set; }
        public string Description { get; }
        public string Caption { get; }
        public List<UrlImage> Images { get; set; }
    }
}