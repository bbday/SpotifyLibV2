using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Helpers
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
            var uri = jsonObject["uri"]?.ToString();
            var id = uri.UriToIdConverter();

            ISpotifyItem item = id.AudioType switch
            {
                AudioItemType.Artist => new SimpleArtist(),
                AudioItemType.Album => new SimpleAlbum(),
                AudioItemType.Playlist => new SimplePlaylist(),
                AudioItemType.Show => new SimpleShow(),
                AudioItemType.Link => (new LinkId(jsonObject["uri"]?.ToString())).LinkType switch
                {
                    LinkType.CollectionTracks => new SimplePlaylist(true),
                    _ => new EmptyItem(jsonObject)
                },
                AudioItemType.Episode => new FullEpisode(),
                AudioItemType.Track => new FullTrack(),
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
