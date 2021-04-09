using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    public class BfsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                var arr = JArray.Load(reader);
                if (arr == null) return null;
                var l = new List<Bfs>();
                foreach (var j in arr)
                {

                    var t = j["id"]?.ToString() switch
                    {
                        "bfs-cell" => new BfsCell(),
                        "bfs-section" => new BfsSection(),
                        "bfs-section-carousel" => new BfsSectionCarousel(),
                        "bfs-grid" => new BfsGrid(),

                        "bfs-card-link" => new BfsCardLink(),
                        "bfs-card-show" => new BfsCardShow(),
                        "bfs-card-playlist" => new BfsCardPlaylist(),
                        "bfs-puff" => new BfsPuff(),
                        "bfs-card-album" => new BfsCardAlbum(),
                        _ => new BfsCell() as Bfs
                    };
                    serializer.Populate(j.CreateReader(), t);
                    l.Add(t);
                }

                return l;
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}