using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleArtist : ISpotifyItem
    {
        private IAudioId __id;

        public AudioType Type => AudioType.Artist;
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

        public string Description
        {
            get => "Artist";
        }
        public string Href { get; set; }
        public string Uri { get; set; }
        public AudioService AudioService => AudioService.Spotify;

        [JsonIgnore]
        public IAudioId Id => __id ??= new ArtistId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
    }
}
