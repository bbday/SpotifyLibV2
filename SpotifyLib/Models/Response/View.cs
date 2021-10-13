using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyLib.Models.Response
{
    public readonly struct View<T>
    {
        [JsonConstructor]
        public View(string id, string name, string rendering, string tagLine, string type, Content<T> content)
        {
            Id = id;
            Name = name;
            Rendering = rendering;
            TagLine = tagLine;
            Type = type;
            Content = content;
        }

        public Content<T> Content { get; }
        public string Id { get; }
        public string Name { get; }
        public string Rendering { get; }
        [JsonPropertyName("tag_line")] public string TagLine { get; }
        public string Type { get; }
    }
    public readonly struct Content<T>
    {
        [JsonConstructor]
        public Content(IEnumerable<T> items, string href)
        {
            Items = items;
            Href = href;
        }

        public string Href { get; }
        public IEnumerable<T> Items { get; }
    }
}