using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class AlbumsResponse
    {
        [JsonProperty("albums")]
        public List<FullAlbum> Albums { get; set; } = default!;
    }
}