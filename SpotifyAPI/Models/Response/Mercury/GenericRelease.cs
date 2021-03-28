using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response.Mercury
{

    public class MercuryGenericRelease : ISpotifyItem
    {
        private IAudioId _id;
        private List<UrlImage> _images;
        public AudioService AudioService => AudioService.Spotify;
        public AudioType Type => AudioType.Album;

        public List<UrlImage> Images => _images ??= new List<UrlImage>
        {
            new UrlImage
            {
                Url = Cover.Uri
            }
        };

        /// <summary>
        /// Name of the album
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Album Cover
        /// </summary>
        [JsonPropertyName("cover")]
        public MercuryCover Cover { get; set; }

        public string Uri { get; set; }
        public IAudioId Id => _id ??= new AlbumId(Uri);

        public virtual string Description { get; }
    }
}
