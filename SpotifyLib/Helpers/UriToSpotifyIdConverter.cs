using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpotifyLib.Models;

namespace SpotifyLib.Helpers
{
    public class UriToSpotifyIdConverter : JsonConverter<SpotifyId>
    {
        public override SpotifyId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new(reader.GetString());
        public override void Write(Utf8JsonWriter writer, SpotifyId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Uri);
        }
    }
}

