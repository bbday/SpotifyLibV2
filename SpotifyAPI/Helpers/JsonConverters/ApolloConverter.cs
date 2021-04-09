using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Response.Mercury.Apollo;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    public class ApolloConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var arr = new List<IApolloHubItem>();
            var jarr = JArray.Load(reader);
            foreach (var jsonObject in jarr)
            {
                var component = jsonObject["component"]["id"]?.ToString();
                IApolloHubItem item = new EmptyItem(jsonObject);
                switch (component)
                {
                    case "glue2:sectionHeader":
                        item = new Header();
                        serializer.Populate(jsonObject.CreateReader(), item);
                        break;
                    case "glue2:card":
                        item = new Station(jsonObject as JObject);
                        break;
                }

                arr.Add(item);
            }

            return arr;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IApolloHubItem);
        }


    }
}
