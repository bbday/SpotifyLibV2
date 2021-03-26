using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleArtist : ISpotifyItem
    {
        private IAudioId __id;

        public AudioType Type { get; set; }
        public List<UrlImage> Images { get; set; }
        public string Name { get; set; }

        public string Description
        {
            get => "Artist";
            set => throw new NotImplementedException();
        }
        public string Href { get; set; }
        public string Uri { get; set; }
        [JsonIgnore]
        public IAudioId Id => __id ??= new ArtistId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
    }
}
