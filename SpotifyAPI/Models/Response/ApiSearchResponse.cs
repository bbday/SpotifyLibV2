using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Response.Mercury.Search;
using SpotifyLibrary.Models.Response.Paging;

namespace SpotifyLibrary.Models.Response
{
    public class ApiSearchResponse
    {
        [JsonProperty("tracks")]
        [CanBeNull]
        public Paging<IAudioItem> Tracks { get; set; }

        [JsonProperty("albums")]
        [CanBeNull]
        public Paging<IAudioItem> Albums { get; set; }

        [JsonProperty("artists")]
        [CanBeNull]
        public Paging<IAudioItem> Artists { get; set; }

        [JsonProperty("playlists")]
        [CanBeNull]
        public Paging<IAudioItem> Playlists { get; set; }

        [JsonProperty("profiles")]
        [CanBeNull]
        public Paging<IAudioItem> Profiles { get; set; }

        [JsonProperty("genres")]
        [CanBeNull]
        public Paging<IAudioItem> Genres { get; set; }

        [JsonProperty("shows")]
        [CanBeNull]
        public Paging<IAudioItem> Shows { get; set; }

        [JsonProperty("audioepisodes")]
        [CanBeNull]
        public Paging<IAudioItem> Audioepisodes { get; set; }
    }
}
