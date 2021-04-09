using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
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

        public AudioService Service => AudioService.Spotify;

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        public List<UrlImage> Images { get; set; }
        public AudioType Type => AudioType.User;
    }
}
