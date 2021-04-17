using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class MercuryAlbum : ISpotifyItem
    {
        private DateTime? _releaseDt;
        private List<UrlImage> _images;
        private IAudioId _id;
        public string Uri { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Album Cover
        /// </summary>
        [JsonPropertyName("cover")]
        public MercuryCover Cover { get; set; }
        public List<UrlImage> Images => _images ??= new List<UrlImage>(1)
        {
            new UrlImage
            {
                Url = Cover.Uri
            }
        };
        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public string Lable { get; set; }

        public List<DiscographyDisc> Discs { get; set; }
        public List<QuickArtist> Artists { get; set; }
        public AlbumsRelatedObject Related { get; set; }
        public List<string> Copyrights { get; set; }

        public string Description => ReleaseDateAsDateTime.ToString("Y");

        public DateTime ReleaseDateAsDateTime => new DateTime(Year, Month ?? 1, Day ?? 1);
        public AudioServiceType AudioService => AudioServiceType.Spotify;
        public IAudioId Id => _id ??= new AlbumId(Uri);

        [System.Text.Json.Serialization.JsonIgnore]
        public AudioItemType Type => AudioItemType.Album;

        [JsonPropertyName("type")]
        public string AlbumType { get; set; }

        [JsonPropertyName("additional")]
        public AdditionalReleases Additional { get; set; }
    }

    public class AdditionalReleases
    {
        [JsonProperty("releases")]
        [JsonPropertyName("releases")]
        public List<MercuryRelease> Releases { get; set; }
    }

    public class AlbumsRelatedObject
    {
        public List<MercuryRelease> Releases { get; set; }
    }
}
