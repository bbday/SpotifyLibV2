using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response.SpotItems.SimpleItems;

namespace Spotify.Lib.Helpers
{
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
                    return arr.Select(z => GetItem(z as JObject, ref serializer)).ToList();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ISpotifyItem GetItem(JObject? jsonObject, ref JsonSerializer serializer)
        {
            var uri = jsonObject["uri"]?.ToString();
            var id = uri.UriToIdConverter();
            if (id == null)
            {
                //try with capital U
                uri = jsonObject["Uri"]?.ToString();
                id = uri.UriToIdConverter();
                if (id == null)
                {
                    Debugger.Break();
                }
            }
            ISpotifyItem item = id.AudioType switch
            {
                AudioItemType.Artist => new SimpleArtist(),
                AudioItemType.Album => new SimpleAlbum(),
                AudioItemType.Playlist => new SimplePlaylist(),
                AudioItemType.Show => new SimpleShow(),
                AudioItemType.Link => new SimpleLink(),
                _ => new EmptyItem()
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
