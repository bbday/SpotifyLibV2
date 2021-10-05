using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SearchItems
{
    public readonly struct SearchAlbum : ISpotifyItem
    {
        [JsonConstructor]
        public SearchAlbum(SpotifyId uri,
            string name,
            string image, List<Quick> artists)
        {
            Name = name;
            Uri = uri;
            Image = image;
            Artists = artists;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public string Image { get; }
        public List<Quick> Artists { get; }
    }

    //public readonly struct Quick
    //{
    //    [JsonConstructor]
    //    public Quick(SpotifyId uri, string name)
    //    {
    //        Uri = uri;
    //        Name = name;
    //    }

    //    [JsonConverter(typeof(UriToSpotifyIdConverter))]
    //    public SpotifyId Uri { get; }
    //    public string Name { get; }
    //}
}
