using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleAlbum : ISpotifyItem, IComparable<SimpleAlbum>, IComparer, IComparer<SimpleAlbum>
    {
        public AudioServiceType AudioService => AudioServiceType.Spotify;

        private string _description;
        private AlbumId __id;

        public AudioItemType Type => AudioItemType.Album;

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

        public int CompareTo(SimpleAlbum? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Uri, other.Uri, StringComparison.Ordinal);
        }

        public int Compare(object x, object y)
        {
            var s = (SimpleAlbum) x;
            var t = (SimpleAlbum) y;
            if (ReferenceEquals(s,t)) return 0;
            if (ReferenceEquals(null, s)) return 1;
            if (ReferenceEquals(null, t)) return -1;

            return string.Compare(s.Uri, t.Uri, StringComparison.Ordinal);
        }

        public int Compare(SimpleAlbum x, SimpleAlbum y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return string.Compare(x.Uri, y.Uri, StringComparison.Ordinal);
        }
    }
}
