using System;
using System.Collections.Generic;
using System.Text;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;

namespace Spotify.Lib.Models.Response.SpotItems.FullItems
{
    public struct FullArtist : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Artist;
        public ISpotifyId Id => new ArtistId(Uri);
        public string Name { get; set; }
        public string Description { get; }
        public string Caption { get; }
        public List<UrlImage> Images { get; set; }
    }
}
