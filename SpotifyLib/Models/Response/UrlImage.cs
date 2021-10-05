using System.Text.Json.Serialization;

namespace SpotifyLib.Models.Response
{
    public readonly struct UrlImage
    {
        [JsonConstructor]
        public UrlImage(string? url, int? width, int? height)
        {
            Url = url;
            Width = width;
            Height = height;
        }

        public string? Url { get; }
        public int? Width { get; }
        public int? Height { get; }
    }
    public readonly struct UriImage
    {
        [JsonConstructor]
        public UriImage(string? uri, int? width, int? height)
        {
            Uri = uri;
            Width = width;
            Height = height;
        }

        public string? Uri { get; }
        public int? Width { get; }
        public int? Height { get; }
    }
}
