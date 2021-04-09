using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    public class SnapshotIdToRevisionidConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var id = JObject.Load(reader);
            return new RevisionId(id?.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}
