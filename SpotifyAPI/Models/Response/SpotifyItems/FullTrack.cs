using System;
using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers.JsonConverters;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullTrack : SimpleTrack, IAlbumTrack
    {
        public override List<UrlImage> Images => Group.Images;
        public override string Description => string.Join(",", Artists.Select(z => z.Name));
        
        [JsonProperty("album")]
        [JsonConverter(typeof(SpotifyItemConverter))]
        public override IAudioItem Group { get; set; }
    }
}
