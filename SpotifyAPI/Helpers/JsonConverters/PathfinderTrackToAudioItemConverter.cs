using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    public class PathfinderTrackToAudioItemConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var jartists = obj["artists"]["items"];

            var artists =
                jartists.Select(jArtist =>
                    new QuickArtist
                    {
                        Name = jArtist["profile"]["name"].ToString(),
                        Uri = jArtist["uri"].ToString()
                    }).ToList();

            var newDiscographyTrack = new DiscographyTrack
            {
                Uri = obj["uri"].ToString(),
                Artists = artists,
                Name = obj["name"].ToString(),
                Playcount = obj["playcount"].ToObject<long>(),
                DiscNumber = obj["discNumber"].ToObject<short>(),
                Number = obj["trackNumber"].ToObject<int>(),
                DurationMs = obj["duration"]["totalMilliseconds"].ToObject<long>(),
                Explicit = false,
                Playable = obj["playability"]["playable"].ToObject<bool>()
            };

            //serializer.Populate(jsonObject.CreateReader(), item);
            return newDiscographyTrack;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
