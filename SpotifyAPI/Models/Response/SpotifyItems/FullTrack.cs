using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullTrack : ISpotifyItem
    {
        private IAudioId _id;
        public AudioService AudioService => AudioService.Spotify;
        [JsonIgnore]
        public IAudioId Id => _id ??= new TrackId(Uri);
        public AudioType Type { get; set; }
        public List<UrlImage> Images => Album.Images;
        public string Name { get; set; }
        public string Description => string.Join(",", Artists.Select(z => z.Name));
        public string Href { get; set; }
        public string Uri { get; set; }

        [JsonProperty("id")]
        public string __id { get; set; }

        public List<SimpleArtist> Artists { get; set; }

        public SimpleAlbum Album { get; set; }
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }
        [JsonProperty("is_playable")]
        public bool CanPlay { get; set; }
    }
}
