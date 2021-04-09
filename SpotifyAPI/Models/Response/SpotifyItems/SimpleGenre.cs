using System;
using System.Collections.Generic;
using System.Text;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleGenre : ISpotifyItem
    {
        private IAudioId __id;
        public AudioService AudioService => AudioService.Spotify;
        [JsonIgnore] public IAudioId Id => __id ??= new LinkId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
        public AudioType Type => AudioType.Link;
        private List<UrlImage> _images;

        public SimpleGenre(JObject jsonObject)
        {
           
        }

        public List<UrlImage> Images
        {
            get
            {
                if (_images == null)
                {
                    if (Image != null)
                        _images = new List<UrlImage>(1)
                        {
                            new UrlImage
                            {
                                Url = Image
                            }
                        };
                }

                return _images;
            }
            set => _images = value;
        }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; }
        public string Uri { get; set; }
    }
}
