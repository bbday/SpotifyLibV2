using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace Spotify.Lib
{
    public static class SpotifySearch
    {
        internal static readonly string MainSearch = "hm://searchview/km/v4/search/";
        internal static readonly string QuickSearch = "hm://searchview/km/v3/suggest/";
        public static Task<string> SearchFull(
            this SpotifyClient client, 
            SearchRequest request)
        {
            var name = client.CannonicalUser;
            var country = client.Country;
            var locale = SpotifyConfig.Locale;

            if (request.Username.IsEmpty())
            {
                request = new SearchRequest(request.Query, request.ImageSize, request.Catalogue, request.Country,
                    request.Locale, name, request.Limit);
            }

            if (request.Country.IsEmpty())
            {
                request = new SearchRequest(request.Query, request.ImageSize, request.Catalogue, country,
                    request.Locale, request.Username, request.Limit);
            }

            if (request.Locale.IsEmpty())
            {
                request = new SearchRequest(request.Query, request.ImageSize, request.Catalogue, request.Country,
                    locale, request.Username, request.Limit);
            }
            var searchUrl = $"{Flurl.Url.Combine(MainSearch, HttpUtility.UrlEncode(request.Query, Encoding.UTF8))}" +
                $"?entityVersion=2&limit={request.Limit}&imageSize={HttpUtility.UrlEncode(request.ImageSize, Encoding.UTF8)}&catalogue={HttpUtility.UrlEncode(request.Catalogue, Encoding.UTF8)}" +
                $"&country={HttpUtility.UrlEncode(request.Country, Encoding.UTF8)}" +
                $"&locale={HttpUtility.UrlEncode(request.Locale, Encoding.UTF8)}" +
                $"&username={HttpUtility.UrlEncode(request.Username, Encoding.UTF8)}";


            return client.SendAsyncReturnJson<string>(
                RawMercuryRequest.Get(searchUrl), CancellationToken.None,
                false);
        }

        public readonly struct SearchRequest
        {
            public readonly int Limit;
            public readonly string ImageSize;
            public readonly string Catalogue;
            public readonly string Country;
            public readonly string Locale;
            public readonly string Username;
            public readonly string Query;
            public SearchRequest(
                string query,
                string imageSize,
                string catalogue,
                string country,
                string locale,
                string name,
                int limit = 4)
            {
                Query = query.Trim();
                Limit = limit;
                ImageSize = imageSize;
                Catalogue = catalogue;
                Country = country;
                Locale = locale;
                Username = name;
            }
        }
    }
    public struct FullSearchResponse 
    {
        [J("results")] public Results Results { get; set; }

        [J("requestId")] public string RequestId { get; set; }

        [J("categoriesOrder")] public List<string> CategoriesOrder { get; set; }
    }
    public struct Results
    {
        [J("tracks")]
        public HitsObject<ISpotifyItem> Tracks { get; set; }

        [J("albums")]
        public HitsObject<ISpotifyItem> Albums { get; set; }

        [J("artists")]
        public HitsObject<ISpotifyItem> Artists { get; set; }

        [J("playlists")]
        public HitsObject<ISpotifyItem> Playlists { get; set; }

        [J("profiles")]
        public HitsObject<ISpotifyItem> Profiles { get; set; }

        [J("genres")]
        public HitsObject<ISpotifyItem> Genres { get; set; }

        [J("topHit")]
        public HitsObject<ISpotifyItem> TopHit { get; set; }

        [J("shows")]
        public HitsObject<ISpotifyItem> Shows { get; set; }

        [J("audioepisodes")]
        public HitsObject<ISpotifyItem> Audioepisodes { get; set; }

        [J("topRecommendations")]
        public HitsObject<ISpotifyItem> TopRecommendations { get; set; }
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
