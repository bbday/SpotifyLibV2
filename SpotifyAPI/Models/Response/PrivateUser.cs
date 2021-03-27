using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.SpotifyItems;

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
