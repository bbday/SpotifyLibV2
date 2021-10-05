using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpotifyLib.Enums;
using SpotifyLib.Models;
using SpotifyLib.Models.Response.SimpleItems;

namespace SpotifyLib.Helpers
{
    public class SpotifyItemConverter : JsonConverter<ISpotifyItem>
    {
        public static JsonSerializerOptions opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public override ISpotifyItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var document = JsonDocument.ParseValue(ref reader))
            {
                if (document.RootElement.TryGetProperty("uri", out var uri))
                {
                    var id = new SpotifyId(uri.GetString());
                    var str1 = document.RootElement.ToString();
                    switch (id.Type)
                    {
                        case AudioItemType.Album:
                            return JsonSerializer.Deserialize<SimpleAlbum>(str1, opts);
                        case AudioItemType.Artist:
                            return JsonSerializer.Deserialize<SimpleArtist>(str1, opts);
                        case AudioItemType.Playlist:
                            return JsonSerializer.Deserialize<SimplePlaylist>(str1, opts);
                        case AudioItemType.Episode:
                            return JsonSerializer.Deserialize<SimpleEpisode>(str1, opts);
                        case AudioItemType.Show:
                            return JsonSerializer.Deserialize<SimpleShow>(str1, opts);
                        default:
                            return new EmptyItem(id, document.RootElement.Clone());
                    }
                }
                else
                {
                    Debugger.Break();
                }
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, ISpotifyItem value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
