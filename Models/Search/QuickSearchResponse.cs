using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyLibV2.Models.Search
{
    public class QuickSearch : ISearchResponse
    {
        [JsonPropertyName("sections")] public List<Section> Sections { get; set; }

        [JsonPropertyName("requestId")] public Guid RequestId { get; set; }
        public SearchType SearchType => SearchType.Quick;
    }
    public class Section
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("items")] public List<SearchItem> Items { get; set; }

    }
}