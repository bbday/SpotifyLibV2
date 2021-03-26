using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;

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
        public AudioService AudioService => AudioService.Spotify;

        private IAudioId __id;

        [JsonConverter(typeof(StringEnumConverter))]
        public AudioType Type { get; set; }
        public List<UrlImage> Images { get; set; }
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
