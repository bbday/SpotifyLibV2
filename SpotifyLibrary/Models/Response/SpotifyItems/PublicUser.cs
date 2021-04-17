using System.Collections.Generic;
using System.Linq;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class PublicUser : IAudioUser
    {
        [JsonProperty("id")]
        public string _id { get; set; }
        public string Uri { get; set; }
        [JsonIgnore]
        public IAudioId Id { get; set; }

        public AudioServiceType Service => AudioServiceType.Spotify;

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        public List<UrlImage> Images { get; set; }
        public AudioItemType Type => AudioItemType.User;
    }
}
