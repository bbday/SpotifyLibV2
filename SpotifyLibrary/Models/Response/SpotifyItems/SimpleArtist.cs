using System.Collections.Generic;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleArtist : ISpotifyItem
    {
        private List<UrlImage>? _images;

        private IAudioId __id;

        public AudioItemType Type => AudioItemType.Artist;
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

        public string Description
        {
            get => "Artist";
        }
        public string Href { get; set; }
        public string Uri { get; set; }
        public AudioServiceType AudioService => AudioServiceType.Spotify;

        [JsonIgnore]
        public IAudioId Id => __id ??= new ArtistId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
    }
}
