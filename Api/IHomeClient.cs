using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Response;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Api
{

    [BaseUrl("https://api.spotify.com")]
    public interface IHomeClient
    {
        [Get("/v1/views/desktop-home")]
        Task<HomeResponse> GetHome(HomeRequest request);

        [Get("/v1/views/{id}")]
        Task<HomeResponseTrimmed> GetTagLineDetailed(string id, [AliasAs("locale")] string locale);

        [Get("/v1/views/{id}")]
        Task<GenericView> GetGenericView([AliasAs("country")] string? Country,

            [AliasAs("locale")] string? Locale,

            [AliasAs("market")] string? market,

            [AliasAs("timestamp")] string? timestamp,

            [AliasAs("platform")] string? platform,

            [AliasAs("content_limit")] int? content_limit,

            [AliasAs("offset")] int? offset,
            [AliasAs("limit")] int? limit,
            [AliasAs("types")] string? type,
            [AliasAs("image_style")] string? image_style,
            string id);
        [Get("/v1/views/{id}")]
        Task<GenericView> GetGenericView(string id, HomeRequest request);
    }

    public class GenericView
    {
        [JsonPropertyName("content")] public GenericViewContent Content { get; set; }

        [JsonPropertyName("external_urls")] public object ExternalUrls { get; set; }

        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("images")] public IEnumerable<SpotifyImage> Images { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("rendering")] public string Rendering { get; set; }

        [JsonPropertyName("tag_line")] public string TagLine { get; set; }

        [JsonPropertyName("type")] public string Type { get; set; }
    }

    public class GenericViewContent
    {
        [JsonPropertyName("href")] public Uri Href { get; set; }

        [JsonPropertyName("items")] public IEnumerable<FluffyItem> Items { get; set; }

        [JsonPropertyName("limit")] public long? Limit { get; set; }

        [JsonPropertyName("next")] public object Next { get; set; }

        [JsonPropertyName("offset")] public long? Offset { get; set; }

        [JsonPropertyName("previous")] public object Previous { get; set; }

        [JsonPropertyName("total")] public long? Total { get; set; }
    }
}
