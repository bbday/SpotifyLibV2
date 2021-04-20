using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Extensions;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models.Response.SpotifyItems;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

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
            var id = uri?.UriToIdConverter();
           
            var item = id?.AudioType switch
            {
                AudioItemType.Episode => new SimpleEpisode(),
                AudioItemType.Artist => new SimpleArtist(),
                AudioItemType.Album => new SimpleAlbum(),
                AudioItemType.Playlist => new SimplePlaylist(jsonObject),
                AudioItemType.Show => new SimpleShow(),
                AudioItemType.Link => (new LinkId(jsonObject["uri"]?.ToString())).LinkType switch
                {
                    LinkType.CollectionTracks => new SimplePlaylist(true),
                    LinkType.Genre => new SimpleGenre(jsonObject),
                    LinkType.DailyMixHub => new SimpleGenre(jsonObject),
                    _ => new EmptyItem(jsonObject)
                },
                AudioItemType.Track => new FullTrack() as ISpotifyItem,
                _ => new EmptyItem(jsonObject) as ISpotifyItem
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
