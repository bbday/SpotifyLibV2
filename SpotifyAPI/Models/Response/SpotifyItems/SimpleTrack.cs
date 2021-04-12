using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers.JsonConverters;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Mercury;
using SpotifyProto;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class SimpleTrack : GenericSpotifyTrack, 
        ISpotifyItem, 
        IAlbumTrack
    {

        private string _description;
        private List<UrlImage> _images;
        public string Image { get; set; }
        public override string Description => _description ??= string.Join(",", Artists.Select(z => z.Name));
        public string Href { get; set; }
        public TrackType TrackType => TrackType.AlbumTrack;
        public TimeSpan? DurationTs => TimeSpan.FromMilliseconds(DurationMs);
        [JsonConverter(typeof(SpotifyItemConverter))]
        public List<IAudioItem> Artists { get; set; }
        [JsonProperty("track_number")]
        public int Index { get; }

        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }
        [JsonProperty("is_playable")]
        public bool CanPlay { get; set; }

        public virtual IAudioItem Group { get; set; }
        public virtual long? Playcount { get; }

        public override List<UrlImage> Images
        {
            get => _images ??= new List<UrlImage>
            {
                new UrlImage
                {
                    Url = Image
                }
            };
        }
        public bool IsDownloaded { get; set; }
    }
}
