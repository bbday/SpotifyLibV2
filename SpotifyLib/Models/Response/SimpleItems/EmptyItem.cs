using System.Text.Json;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{

    public readonly struct EmptyItem : ISpotifyItem
    {
        public EmptyItem(SpotifyId uri, JsonElement element)
        {
            Uri = uri;
            Element = element;
        }
        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public JsonElement Element { get; }
    }
}