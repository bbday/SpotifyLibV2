using System;
using System.Text.Json;
using SpotifyLib.Models;
using SpotifyLib.Models.Response;

namespace SpotifyLib.Helpers
{
    public static class ViewDeserializerHelpers
    {
        public static JsonSerializerOptions opts = new JsonSerializerOptions
        {
            Converters =
            {
                new SpotifyItemConverter()
            },
            PropertyNameCaseInsensitive = true
        };
        public static View<View<ISpotifyItem>> Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<View<View<ISpotifyItem>>>(new ReadOnlySpan<byte>(data),
                opts);
        }

        public static View<ISpotifyItem> DeserializeSingleView(byte[] data)
        {
            return JsonSerializer.Deserialize<View<ISpotifyItem>>(new ReadOnlySpan<byte>(data),
                opts);
        }
    }
}