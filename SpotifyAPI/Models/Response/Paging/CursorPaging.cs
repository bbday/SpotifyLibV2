using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Response.Paging
{
    public class CursorPaging<T> : IPaginatable<T>
    {
        public string Href { get; set; } = default!;
        [JsonProperty("items")]
        public List<T>? Items { get; set; } = default!;
        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("next")]
        public string? Next { get; set; } = default!;
        [JsonProperty("cursors")]
        public Cursor Cursors { get; set; } = default!;
        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class CursorPaging<T, TNext> : IPaginatable<T, TNext>
    {
        public string Href { get; set; } = default!;
        [JsonProperty("items")]
        public List<T>? Items { get; set; } = default!;
        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("next")]
        public string? Next { get; set; } = default!;
        [JsonProperty("cursors")]
        public Cursor Cursors { get; set; } = default!;
        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Cursor
    {
        [JsonProperty("before")]
        public string Before { get; set; } = default!;
        [JsonProperty("after")]
        public string After { get; set; } = default!;
    }
}
