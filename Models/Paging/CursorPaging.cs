using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyLib.Models.Api.Paging
{
    public class CursorPaging<T> : IPaginatable<T>
    {
        public string Href { get; set; } = default!;
        [JsonPropertyName("items")]
        public List<T>? Items { get; set; } = default!;
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        [JsonPropertyName("next")]
        public string? Next { get; set; } = default!;
        [JsonPropertyName("cursors")]
        public Cursor Cursors { get; set; } = default!;
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class CursorPaging<T, TNext> : IPaginatable<T, TNext>
    {
        public string Href { get; set; } = default!;
        [JsonPropertyName("items")]
        public List<T>? Items { get; set; } = default!;
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        [JsonPropertyName("next")]
        public string? Next { get; set; } = default!;
        [JsonPropertyName("cursors")]
        public Cursor Cursors { get; set; } = default!;
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class Cursor
    {
        [JsonPropertyName("before")]
        public string Before { get; set; } = default!;
        [JsonPropertyName("after")]
        public string After { get; set; } = default!;
    }
}
