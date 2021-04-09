using System;
using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.SpotifyItems;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    //public class SpotifyItemConverterTextJson : System.Text.Json.Serialization.JsonConverter<ISpotifyItem>
    //{
    //    private static Type _spotifyItemType;
    //    private static Type ISpotifyItemType => _spotifyItemType ??= typeof(ISpotifyItem);
    //    public override bool CanConvert(Type typeToConvert)
    //    {
    //        return typeof(ISpotifyItem).IsAssignableFrom(typeToConvert);
    //    }

    //    public override ISpotifyItem? Read(ref Utf8JsonReader reader, 
    //        Type typeToConvert, 
    //        JsonSerializerOptions options)
    //    {
    //        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
    //        {
    //            if (jsonDoc.RootElement.TryGetProperty("type", out var typeValue))
    //            {
    //                if (!System.Enum.TryParse<AudioType>(typeValue.ToString(), true, out var typ))
    //                    return new EmptyItem();
    //                ISpotifyItem item = typ switch
    //                {
    //                    AudioType.Artist => new SimpleArtist(),
    //                    AudioType.Album => new SimpleAlbum(),
    //                    AudioType.Playlist => new SimplePlaylist(),
    //                    AudioType.Link => (new LinkId(jsonDoc.RootElement.GetProperty("uri").ToString())).LinkType switch
    //                    {
    //                        LinkType.CollectionTracks => new SimplePlaylist(true),
    //                        _ => new EmptyItem()
    //                    },
    //                    _ => new EmptyItem()
    //                };
    //                var m = Map(item, jsonDoc.ToString());
    //                return m;
    //            }
    //            else
    //                return new EmptyItem();
    //        }
    //        throw new NotImplementedException();
    //    }

    //    public override void Write(Utf8JsonWriter writer, ISpotifyItem value, JsonSerializerOptions options)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public static T Map<T>(T obj, string jsonString) where T : class
    //    {
    //        var newObj = System.Text.Json.JsonSerializer.Deserialize<T>(jsonString);

    //        foreach (var property in newObj.GetType().GetProperties())
    //        {
    //            if (obj.GetType().GetProperties().Any(x => x.Name == property.Name && property.GetValue(newObj) != null))
    //            {
    //                if (property.GetType().IsClass && property.PropertyType.Assembly.FullName == typeof(T).Assembly.FullName)
    //                {
    //                    var mapMethod = typeof(SpotifyItemConverterTextJson).GetMethod("Map");
    //                    var genericMethod = mapMethod.MakeGenericMethod(property.GetValue(newObj).GetType());
    //                    var obj2 = genericMethod.Invoke(null, new object[] { property.GetValue(newObj),
    //                        System.Text.Json.JsonSerializer.Serialize(property.GetValue(newObj)) });

    //                    foreach (var property2 in obj2.GetType().GetProperties())
    //                    {
    //                        if (property2.GetValue(obj2) != null)
    //                        {
    //                            property.GetValue(obj).GetType().GetProperty(property2.Name).SetValue(property.GetValue(obj), property2.GetValue(obj2));
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    property.SetValue(obj, property.GetValue(newObj));
    //                }
    //            }
    //        }

    //        return obj;
    //    }
    //}
    public class SpotifyItemConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var jsonObject = JObject.Load(reader);
                    return GetItem(jsonObject, ref serializer);
                case JsonToken.StartArray:
                    var arr = JArray.Load(reader);
                    var dt = 
                        arr.Select(item => GetItem(item as JObject, ref serializer)).ToList();
                    return dt;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private IAudioItem GetItem(JObject jsonObject, ref JsonSerializer serializer)
        {
            if (!System.Enum.TryParse<AudioType>(jsonObject["type"]?.ToString(), true, out var typ))
                return new EmptyItem(jsonObject);
            ISpotifyItem item = typ switch
            {
                AudioType.Artist => new SimpleArtist(),
                AudioType.Album => new SimpleAlbum(),
                AudioType.Playlist => new SimplePlaylist(),
                AudioType.Show => new SimpleShow(),
                AudioType.Link => (new LinkId(jsonObject["uri"]?.ToString())).LinkType switch
                {
                    LinkType.CollectionTracks => new SimplePlaylist(true),
                    _ => new EmptyItem(jsonObject)
                },
                _ => new EmptyItem(jsonObject)
            };
            serializer.Populate(jsonObject.CreateReader(), item);
            return item;
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ISpotifyItem)
                || objectType == typeof(List<ISpotifyItem>);
        }


    }
}
