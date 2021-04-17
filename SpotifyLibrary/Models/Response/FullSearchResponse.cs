using System.Collections.Generic;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Helpers.JsonConverters;
using SpotifyLibrary.Interfaces;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace SpotifyLibrary.Models.Response
{
    public class FullSearchResponse : ISearchResponse
    {
        [J("results")] public Results Results { get; set; }

        [J("requestId")] public string RequestId { get; set; }

        [J("categoriesOrder")] public List<string> CategoriesOrder { get; set; }

        public SearchType SearchType => SearchType.Full;
    }
    public class Results
    {
        [J("tracks")]
        public HitsObject<IAudioItem> Tracks { get; set; }

        [J("albums")]
        public HitsObject<IAudioItem> Albums { get; set; }

        [J("artists")]
        public HitsObject<IAudioItem> Artists { get; set; }

        [J("playlists")]
        public HitsObject<IAudioItem> Playlists { get; set; }

        [J("profiles")]
        public HitsObject<IAudioItem> Profiles { get; set; }

        [J("genres")]
        public HitsObject<IAudioItem> Genres { get; set; }

        [J("topHit")]
        public HitsObject<IAudioItem> TopHit { get; set; }

        [J("shows")]
        public HitsObject<IAudioItem> Shows { get; set; }

        [J("audioepisodes")]
        public HitsObject<IAudioItem> Audioepisodes { get; set; }

        [J("topRecommendations")]
        public HitsObject<IAudioItem> TopRecommendations { get; set; }
    }
    public class HitsObject<T>
    {
        [J("hits")]
        [JsonConverter(typeof(MercuryTypeConverterToIAudioItem))]
        public List<T> Hits { get; set; }
        [J("total")]
        public long Total { get; set; }
        public int Count => Hits.Count;
    }
}