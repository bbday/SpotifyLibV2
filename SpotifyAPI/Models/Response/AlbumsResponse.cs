using System;
using System.Collections.Generic;
using System.Text;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response
{
    public class AlbumsResponse
    {
        [JsonProperty("albums")]
        public List<FullAlbum> Albums { get; set; } = default!;
    }
}
