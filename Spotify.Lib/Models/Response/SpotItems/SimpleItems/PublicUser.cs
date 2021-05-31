using System.Collections.Generic;
using Newtonsoft.Json;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public class PublicUser : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Playlist;
        public ISpotifyId Id => new UserId(Uri);

        [JsonProperty("display_name")] public string Name { get; set; }

        public string Description { get; }
        public string Caption { get; }
        public List<UrlImage> Images { get; set; }
    }
}