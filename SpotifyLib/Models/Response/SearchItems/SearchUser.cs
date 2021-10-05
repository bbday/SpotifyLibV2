using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SearchItems
{
    public readonly struct SearchUser : ISpotifyItem
    {
        [JsonConstructor]
        public SearchUser(SpotifyId uri,
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