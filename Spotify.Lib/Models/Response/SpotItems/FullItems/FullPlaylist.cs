using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response.SpotItems.SimpleItems;

namespace Spotify.Lib.Models.Response.SpotItems.FullItems
{
    public struct Followers
    {
        public int Total { get; set; }
    }

    public struct FullPlaylist : ISpotifyItem
    {
        public Followers Followers { get; set; }
        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Playlist;
        public ISpotifyId Id => new PlaylistId(Uri);
        public string Name { get; set; }
        public string Description { get; set; }
        public string Caption => $"Playlist • {Owner.Name}";
        public List<UrlImage> Images { get; set; }
        public bool Collaborative { get; set; }
        public PublicUser Owner { get; set; }
    }
}