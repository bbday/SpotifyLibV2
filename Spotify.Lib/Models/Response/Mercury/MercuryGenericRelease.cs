using System.Collections.Generic;
using System.Text.Json.Serialization;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response.SpotItems;

namespace Spotify.Lib.Models.Response.Mercury
{
    public class MercuryGenericRelease : ISpotifyItem
    {
        private ISpotifyId _id;
        private List<UrlImage> _images;
        public AudioItemType Type => AudioItemType.Album;

        public string Caption => Type.ToString();

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
        public ISpotifyId Id => _id ??= new AlbumId(Uri);

        public virtual string Description { get; }
    }
}