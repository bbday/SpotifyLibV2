using System.Collections.Generic;
using System.Linq;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleGenre : ISpotifyItem
    {
        private List<UrlImage>? _images;

        private LinkId? _proxyId;
        public AudioServiceType AudioService => AudioServiceType.Spotify;
        [JsonIgnore] public IAudioId Id => _proxyId ??= new LinkId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
        public AudioItemType Type => AudioItemType.Link;

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
        public string? Image { get; set; }
        public string Name { get; set; }
        public string Description { get; }
        public string Uri { get; set; }
    }
}
