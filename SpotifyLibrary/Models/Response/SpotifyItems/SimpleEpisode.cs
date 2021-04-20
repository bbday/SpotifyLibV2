using System.Collections.Generic;
using MediaLibrary;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleEpisode : GenericSpotifyTrack
    {
        private List<UrlImage> _images;
        public override List<UrlImage> Images
        {
            get
            {
                if (_images == null)
                {
                    if (Image != null)
                        _images = new List<UrlImage>(1)
                        {
                            new UrlImage
                            {
                                Url = Image
                            }
                        };
                }

                return _images;
            }
            set => _images = value;
        }
        public string Image { get; set; }

        public override IAudioId Id => _id ??= new EpisodeId(Uri);
    }
}
