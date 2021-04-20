using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class PrivateUser : IAudioUser
    {
        private IAudioId __id;
        public string Country { get; set; }
        public AudioServiceType Service => AudioServiceType.Spotify;

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public string Email { get; set; }
        [JsonProperty("explicit_content")]
        public ExplicitContent ExplicitContent { get; set; }
        public Followers Folowers { get; set; }
        [JsonIgnore] public IAudioId Id => __id ??= new UserId(Uri);
        public List<UrlImage> Images { get; set; }
        public AudioItemType Type { get; set; }
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
