using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response
{
    public class PrivateUser : IAudioUser
    {
        private IAudioId __id;
        public string Country { get; set; }
        public AudioService Service => AudioService.Spotify;

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Email { get; set; }
        [JsonProperty("explicit_content")]
        public ExplicitContent ExplicitContent { get; set; }
        public Followers Folowers { get; set; }
        [JsonIgnore] public IAudioId Id => __id ??= new UserId(_id);
        public List<UrlImage> Images { get; set; }
        public AudioType Type { get; set; }
        public string Uri { get; set; }
        [JsonProperty("id")]
        public string _id { get; set; }
    }

    public class ExplicitContent
    {
        [JsonProperty("filter_enabled")]
        public bool FilterEnabled { get; set; }
        [JsonProperty("filter_locked")]
        public bool FilterLocked { get; set; }
    }
}
