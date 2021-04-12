using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    public class MercuryTypeConverterToIAudioItem : JsonConverter
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
                AudioType.Episode => new SimpleEpisode(),
                AudioType.Artist => new SimpleArtist(),
                AudioType.Album => new SimpleAlbum(),
                AudioType.Playlist => new SimplePlaylist(jsonObject),
                AudioType.Show => new SimpleShow(),
                AudioType.Link => (new LinkId(jsonObject["uri"]?.ToString())).LinkType switch
                {
                    LinkType.CollectionTracks => new SimplePlaylist(true),
                    LinkType.Genre => new SimpleGenre(jsonObject),
                    _ => new EmptyItem(jsonObject)
                },
                AudioType.Track => new SimpleTrack(),
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
