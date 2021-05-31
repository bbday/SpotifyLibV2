using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct SimpleShow : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Show;
        public ISpotifyId Id => new ShowId(Uri);
        public string Name { get; set; }
        public string Description { get; set; }
        public string Caption => Copyrights != null ? 
            string.Join(", ", Copyrights?.Select(z => z.Text) ?? Array.Empty<string>()) : "";
        public string Publisher { get; set; }
        public List<UrlImage> Images
        {
            get;
            set;
        }
        public List<Copyright> Copyrights { get; set; }
        [JsonProperty("is_explicit")] public bool Explicit { get; set; }
    }

    public struct Copyright
    {
        public string Text { get; set; }
        public string Type { get; set; }
    }
}