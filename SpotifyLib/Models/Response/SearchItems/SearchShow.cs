using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SearchItems
{
    public readonly struct SearchShow : ISpotifyItem
    {
        [JsonConstructor]
        public SearchShow(SpotifyId uri,
            string name,
            string image)
        {
            Name = name;
            Uri = uri;
            Image = image;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public string Image { get; }
    }
}
