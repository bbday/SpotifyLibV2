using System.Collections.Generic;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimplePlaylist : ISpotifyItem
    {
        private bool _isLink;

        public SimplePlaylist()
        {
            _isLink = false;
        }
        public SimplePlaylist(bool isLink)
        {
            _isLink = isLink;
        }
        public AudioServiceType AudioService => AudioServiceType.Spotify;

        private IAudioId __id;

        public AudioItemType Type => AudioItemType.Playlist;
        private List<UrlImage> _images;

        public SimplePlaylist(JObject jsonObject)
        {
            var followerCount = jsonObject["followersCount"]?.ToObject<int>();
            var author = jsonObject["author"]?.ToString();
            Description = $"{followerCount:#,##0} followers • {author}";
        }

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
        public IAudioId Id => __id ??= _isLink ? new LinkId(Uri) : new PlaylistId(Uri);
        [JsonProperty("id")]
        public string _id { get; set; }
        [JsonProperty("snapshot_id")]
        public string SnapshotId { get; set; }
        public PublicUser Owner { get; set; }

    }
}
