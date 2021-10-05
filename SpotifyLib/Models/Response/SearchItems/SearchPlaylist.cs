using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SearchItems
{
    public readonly struct SearchPlaylist : ISpotifyItem
    {
        [JsonConstructor]
        public SearchPlaylist(SpotifyId uri,
            string name,
            string image,
            long followers,
            string author)
        {
            Name = name;
            Uri = uri;
            Image = image;
            Followers = followers;
            Author = author;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public string Image { get; }
        public long Followers { get; }
        public string Author { get; }
    }
}
