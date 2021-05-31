using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct SimplePlaylist : ISpotifyItem
    {

        public string Uri { get; set; }
        public AudioItemType Type => AudioItemType.Playlist;
        public ISpotifyId Id => new PlaylistId(Uri);
        public string Name { get; set; }
        public string Description { get; set; }
        public string Caption => $"Playlist • {Owner?.Name}";
        private List<UrlImage> _images;
        private JObject jobj;

        public SimplePlaylist(JObject jobj) : this()
        {
            this.jobj = jobj;
        }

        public List<UrlImage> Images
        {
            get
            {
                if (_images != null) return _images;
                if (Image != null)
                    _images = new List<UrlImage>(1)
                    {
                        new UrlImage
                        {
                            Url = Image
                        }
                    };

                return _images;
            }
            set => _images = value;
        }
        public string Image { get; set; }
        public bool Collaborative { get; set; }
        public PublicUser Owner { get; set; }
    }
}