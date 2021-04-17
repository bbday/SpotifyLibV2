using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response
{
    public class ApiSearchResponse
    {
        [JsonProperty("tracks")] public Paging<IAudioItem> Tracks { get; set; }

        [JsonProperty("albums")] public Paging<IAudioItem> Albums { get; set; }

        [JsonProperty("artists")] public Paging<IAudioItem> Artists { get; set; }

        [JsonProperty("playlists")] public Paging<IAudioItem> Playlists { get; set; }

        [JsonProperty("profiles")] public Paging<IAudioItem> Profiles { get; set; }

        [JsonProperty("genres")] public Paging<IAudioItem> Genres { get; set; }

        [JsonProperty("shows")] public Paging<IAudioItem> Shows { get; set; }

        [JsonProperty("audioepisodes")] public Paging<IAudioItem> Audioepisodes { get; set; }
    }
}