using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using SpotifyLibrary.Models.Response.Interfaces;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class DiscographyTrack : PlaycountedTrack, ITrackItem
    {

        /// <summary>
        /// Number from 0 to 100. See the spotify api for more details.
        /// </summary>
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }

        /// <summary>
        /// Hierarchical number of the track.
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; set; }

        /// <summary>
        /// Duration in ms.
        /// </summary>
        [JsonPropertyName("duration")]
        public long DurationMs { get; set; }

        public IAudioItem Group => null;

        /// <summary>
        /// Boolean indicating whether or not the track is playable (region etc).
        /// </summary>
        [JsonPropertyName("playable")]
        public bool Playable { get; set; }

        /// <summary>
        /// List of simple artist objects. See <see cref="QuickArtist"/>
        /// </summary>
        [JsonPropertyName("artists")]
        public IEnumerable<QuickArtist> Artists { get; set; }

        public int DiscNumber { get; set; }

        public TimeSpan? DurationTs => TimeSpan.FromMilliseconds(DurationMs);

        List<IAudioItem> ITrackItem.Artists => Artists.Cast<IAudioItem>()
            .ToList();

        public override string Description => string.Join(", ", 
            Artists.Select(z => z.Name));
    }
}
