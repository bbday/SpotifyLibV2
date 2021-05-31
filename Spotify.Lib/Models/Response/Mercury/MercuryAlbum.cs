using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response.SpotItems;

namespace Spotify.Lib.Models.Response.Mercury
{
    public struct MercuryAlbum
    {
        private ISpotifyId _id;
        public ISpotifyId Id => _id ??= new AlbumId(Uri);
        public AudioItemType Type => AudioItemType.Album;
        public string Name { get; set; }
        public string Uri { get; set; }
        public short? Month { get; set; }
        public short? Day { get; set; }
        public short Year { get; set; }
        public string Label { get; set; }
        [JsonPropertyName("track_count")]
        public int TrackCount { get; set; }
        public UrlImage Cover { get; set; }
        public List<DiscographyDisc> Discs { get; set; }
        public List<string> Copyrights { get; set; }
        public List<QuickArtist> Artists { get; set; }
        public DiscographyWrapper Related { get; set; }
        [JsonPropertyName("additional")]
        public AdditionalReleases? Additional { get; set; }
    }
    public struct AdditionalReleases
    {
        [JsonProperty("releases")]
        [JsonPropertyName("releases")]
        public List<MercuryRelease> Releases { get; set; }
    }
}
