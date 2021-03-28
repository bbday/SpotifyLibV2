using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading;
using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.SpotifyItems;

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
        public AudioService AudioService => AudioService.Spotify;
        public IAudioId Id => _id ??= new AlbumId(Uri);

        [System.Text.Json.Serialization.JsonIgnore]
        public AudioType Type => AudioType.Album;

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
