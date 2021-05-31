using System;
using System.Collections.Generic;
using System.Text;
using MoreLinq;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Response.SpotItems.SimpleItems
{
    public struct SimpleLink : ISpotifyItem
    {
        public string Uri { get; set; }
        public AudioItemType Type { get; set; }
        public ISpotifyId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Caption { get; set; }
        private List<UrlImage> _images;
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
    }
}
