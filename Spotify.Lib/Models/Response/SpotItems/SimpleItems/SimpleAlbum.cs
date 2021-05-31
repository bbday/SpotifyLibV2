using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using SpotifyProto;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct SimpleAlbum : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Album;
        public ISpotifyId Id => new AlbumId(Uri);
        public string Name { get; set; }

        public string Description => string.Join(", ",
            Artists.Select(z => z.Name));

        public string Caption => AlbumType.ToString();
        private List<UrlImage> _images;

        public SimpleAlbum(Album trackAlbum) : this()
        {
            Artists = trackAlbum.Artist.Select(z => new SimpleArtist
            {
                Name = z.Name
            }).ToList();
            Name = trackAlbum.Name;
            Uri = AlbumId.FromGid(trackAlbum.Gid).Uri;
            Images = trackAlbum.CoverGroup.
                Image.Select(z => new UrlImage
                {
                    Url = $"https://i.scdn.co/image/{z.FileId.ToByteArray().BytesToHex().ToLowerInvariant()}",
                    Height = z.HasHeight ? z.Height : null,
                    Width = z.HasWidth ? z.Width : null
                })
                .ToList();
        }


        public List<UrlImage> Images
        {
            get
            {
                if (_images != null) return _images;
                if (Image != null)
                    _images = new List<UrlImage>(1)
                    {
                        new UrlImage
                        {
                            Url = Image
                        }
                    };

                return _images;
            }
            set => _images = value;
        }
        public string Image { get; set; }
        public List<SimpleArtist> Artists { get; set; }
        [JsonProperty("release_date")] public string ReleaseDate { get; set; }

        [JsonProperty("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }

        [JsonProperty("total_tracks")] public short TotalTracks { get; set; }
        [JsonProperty("album_type")] public AlbumType AlbumType { get; set; }
    }
}