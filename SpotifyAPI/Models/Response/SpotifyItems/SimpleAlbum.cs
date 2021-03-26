using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleAlbum : ISpotifyItem
    {
        public AudioService AudioService => AudioService.Spotify;

        private string _description;
        private AlbumId __id;

        public AudioType Type { get; set; }
        public List<UrlImage> Images { get; set; }
        public string Name { get; set; }

        public string Description
        {
            get => _description ??= string.Join(",", Artists.Select(z => z.Name));
            set => throw new NotImplementedException();
        }
        public string Href { get; set; }
        public string Uri { get; set; }
        [JsonIgnore]
        public IAudioId Id => __id ??= new AlbumId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
        public List<SimpleArtist> Artists { get; set; }
    }
}
