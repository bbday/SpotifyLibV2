using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleAlbum : ISpotifyItem
    {
        public AudioService AudioService => AudioService.Spotify;

        private string _description;
        private AlbumId __id;

        public AudioType Type => AudioType.Album;

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
            get => _description ??= string.Join(",", Artists.Select(z => z.Name));
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
