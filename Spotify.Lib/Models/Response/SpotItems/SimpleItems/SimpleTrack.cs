using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using SpotifyProto;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct SimpleTrack : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Track;
        public ISpotifyId Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Caption { get; }
        public List<UrlImage> Images { get; }
    }
}
