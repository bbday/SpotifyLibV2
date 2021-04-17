using System.Collections.Generic;
using System.Linq;
using MediaLibrary;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers;

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
