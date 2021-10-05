using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Flurl;
using SpotifyLib.Enums;
using SpotifyLib.Models;
using SpotifyLib.Models.Response.SearchItems;

namespace SpotifyLib.Helpers
{
    public readonly struct SearchGrouping
    {
        public SearchGrouping(string key, string title, IEnumerable<ISpotifyItem> items)
        {
            Key = key;
            Title = title;
            Items = items;
        }

        public readonly string Key { get; }
        public readonly string Title { get; }
        public readonly IEnumerable<ISpotifyItem> Items { get; }
    }
    public static class MercurySearchHelpers
    {
        internal const string MainSearch = "hm://searchview/km/v4/search/";
        internal const string QuickSearch = "hm://searchview/km/v3/suggest/";

        public static async Task<IEnumerable<SearchGrouping>> SearchQuick(this SpotifyConnectionState client,
            MercurySearchRequest request)
        {
            var quickUrl =
                Flurl.Url.Combine(MainSearch,
                        HttpUtility.UrlEncode(request.Query, Encoding.UTF8))
                    .SetQueryParam("limit", request.Limit)
                    .SetQueryParam("entityVersion", "2")
                    .SetQueryParam("sequence", client.SearchSequence)
                    .SetQueryParam("catalogue", request.Catalogue)
                    .SetQueryParam("country", request.Country)
                    .SetQueryParam("locale", request.Locale)
                    .SetQueryParam("username", request.Username)
                    .SetQueryParam("types", "artist");
            Interlocked.Increment(ref client.SearchSequence);

            var data = await client.SendAndReceiveAsJsonString(quickUrl);

            var asJsonDocument = JsonDocument.Parse(data);

            static IEnumerable<Quick> GetQuickArr(JsonElement.ArrayEnumerator arr)
            {
                while (arr.MoveNext())
                {
                    var item = arr.Current;
                    yield return GetQuick(item);
                }

            }

            static Quick GetQuick(JsonElement elem)
            {
                return new Quick(new SpotifyId(elem.GetProperty("uri").GetString()),
                    elem.GetProperty("name").GetString());
            }
            static string GetImageOrNull(JsonElement elem)
            {
                if (elem.TryGetProperty("image", out var img))
                {
                    if (img.ValueKind != JsonValueKind.Null) return img.GetString();
                }

                return "";
            }


            static IEnumerable<ISpotifyItem> GetItems(JsonElement.ArrayEnumerator data2)
            {
                while (data2.MoveNext())
                {
                    var item = data2.Current;
                    var uri = new SpotifyId(item.GetProperty("uri")
                        .GetString());
                    switch (uri.Type)
                    {
                        case AudioItemType.Track:
                            Debug.WriteLine($"Handling track {uri}");
                            yield return new SearchTrack(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item),
                                TimeSpan.FromMilliseconds(item.GetProperty("duration").GetDouble()),
                                GetQuickArr(item.GetProperty("artists").EnumerateArray()),
                                GetQuick(item.GetProperty("album")));
                            break;
                        case AudioItemType.Episode:
                            Debug.WriteLine($"Handling episode {uri}");
                            yield return new SearchEpisode(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item));
                            break;
                        case AudioItemType.Artist:
                            Debug.WriteLine($"Handling artist {uri}");
                            yield return new SearchArtist(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item));
                            break;
                        case AudioItemType.Album:
                            Debug.WriteLine($"Handling album {uri}");
                            yield return new SearchAlbum(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item),
                                GetQuickArr(item.GetProperty("artists").EnumerateArray()).ToList());
                            break;
                        case AudioItemType.User:
                            Debug.WriteLine($"Handling user {uri}");
                            yield return new SearchUser(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item));
                            break;
                        case AudioItemType.Playlist:
                            Debug.WriteLine($"Handling Playlist {uri}");
                            yield return new SearchPlaylist(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item),
                                item.GetProperty("followersCount").GetInt64(),
                                item.GetProperty("author").GetString());
                            break;
                        case AudioItemType.Link:
                            Debug.WriteLine($"Handling link {uri}");
                            yield return new SearchEpisode(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item));
                            break;
                        case AudioItemType.Show:
                            Debug.WriteLine($"Handling show {uri}");
                            yield return new SearchShow(uri, item.GetProperty("name").GetString(),
                                GetImageOrNull(item));
                            break;
                        default:
                            //TODO! Convert to ISpotifyObject.
                            Debug.WriteLine($"Default to default for {uri}");
                            yield return default(ISpotifyItem);
                            break;
                    }

                }
            }

            IEnumerable<SearchGrouping> GetSearchItems(JsonElement.ArrayEnumerator categoriesOrder)
            {
                var results =
                    asJsonDocument.RootElement.GetProperty("results");
                while (categoriesOrder.MoveNext())
                {
                    var currentCategory = categoriesOrder.Current
                        .GetString();
                    if (!results.TryGetProperty(currentCategory ?? throw new InvalidOperationException(), out var props)) continue;
                    if (!props.TryGetProperty("hits", out var hits)) continue;
                    if (hits.ValueKind != JsonValueKind.Null)
                    {
                        yield return new SearchGrouping(currentCategory, GetTitle(currentCategory),
                            GetItems(hits.EnumerateArray()));
                    }
                }
            }


            return GetSearchItems(
                asJsonDocument.RootElement.GetProperty("categoriesOrder")
                    .EnumerateArray());
        }

        private static string GetTitle(string category)
        {
            return category switch
            {
                "tracks" => "Tracks",
                "albums" => "Albums",
                "artists" => "Artists",
                "playlists" => "Playlists",
                "profiles" => "Users",
                "genres" => "Genres",
                "topHit" => "Top",
                "shows" => "Shows",
                "audioepisodes" => "Podcasts",
                "topRecommendations" => "Recommendations",
                _ => category
            };
        }
    }

    public readonly struct MercurySearchRequest
    {
        public MercurySearchRequest(string query,
            string locale,
            string username,
            string country,
            int limit = 5)
        {
            Query = query;
            Limit = limit;
            Catalogue = "Premium";
            Country = country;
            Username = username;
            Locale = locale;
        }
        public string Query { get; }
        public int Limit { get; }
        public string Catalogue { get; }
        public string Country { get; }
        public string Locale { get; }
        public string Username { get; }
    }
}