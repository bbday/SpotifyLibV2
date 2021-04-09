using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleShow : ISpotifyItem
    {
        public AudioService AudioService => AudioService.Spotify;

        private string _description;
        private ShowId __id;

        public AudioType Type => AudioType.Show;
        private List<UrlImage> _images;
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

        public string Description { get; set; }
        public string Href { get; set; }
        public string Uri { get; set; }
        [JsonIgnore]
        public IAudioId Id => __id ??= new ShowId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
        public List<SimpleArtist> Artists { get; set; }
        public List<string> Languages { get; set; }
        [JsonProperty("media_type")]
        public string MediaType { get; set; }
        public string Publisher { get; set; }
        [JsonProperty("total_episodes")]
        public int TotalEpisodes { get; set; }
        public bool Explicit { get; set; }
    }
}
