using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Helpers.JsonConverters
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
            var jsonObject = JObject.Load(reader);
            if (!System.Enum.TryParse<AudioType>(jsonObject["type"]?.ToString(), true, out var typ))
                return new EmptyItem();
            ISpotifyItem item = typ switch
            {
                AudioType.Artist => new SimpleArtist(),
                AudioType.Album => new SimpleAlbum(),
                AudioType.Playlist => new SimplePlaylist(),
                AudioType.Link => (new LinkId(jsonObject["uri"]?.ToString())).LinkType switch
                {
                    LinkType.CollectionTracks => new SimplePlaylist(true),
                    _ => new EmptyItem()
                },
                _ => new EmptyItem()
            };
            serializer.Populate(jsonObject.CreateReader(), item);
            return item;

        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ISpotifyItem);
        }
    }
}
