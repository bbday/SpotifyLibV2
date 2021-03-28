using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class FullTrack : GenericSpotifyTrack, ITrackItem
    {
        public override List<UrlImage> Images => Album.Images;
        public override string Description => string.Join(",", Artists.Select(z => z.Name));
        public string Href { get; set; }
        public List<SimpleArtist> Artists { get; set; }
        public SimpleAlbum Album { get; set; }
        [JsonProperty("duration_ms")]
        public int DurationMs { get; set; }
        [JsonProperty("is_playable")]
        public bool CanPlay { get; set; }

        public TimeSpan? DurationTs => TimeSpan.FromMilliseconds(DurationMs);
        public IAudioItem Group => Album;
        public long? Playcount => throw new NotImplementedException();

        List<IAudioItem> ITrackItem.Artists => throw new NotImplementedException();
    }
}
