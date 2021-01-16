using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibV2.Models.Shared;

namespace SpotifyLibV2.Models.Response
{
    public class SimpleAlbum : GenericSpotifyItem
    {
        public string AlbumGroup { get; set; } = default!;

        public string AlbumType { get; set; } = default!;

        public List<SimpleArtist> Artists { get; set; } = default!;

        public List<string> AvailableMarkets { get; set; } = default!;

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public string Href { get; set; } = default!;

        public List<SpotifyImage> Images { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string ReleaseDate { get; set; } = default!;

        public string ReleaseDatePrecision { get; set; } = default!;

        public Dictionary<string, string> Restrictions { get; set; } = default!;
    }
}
