using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Response;
using Spotify.Lib.Models.Response.Mercury;

namespace Spotify.Lib.Models.TracksnEpisodes
{
    public class DiscographyTrack : PlaycountedTrack, IAlbumTrack
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

        public ISpotifyItem Group => null;

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

        public short DiscNumber { get; set; }

        public TimeSpan DurationTs => TimeSpan.FromMilliseconds(DurationMs);

        List<ISpotifyItem> ITrackItem.Artists => Artists.Cast<ISpotifyItem>()
            .ToList();

        public override string Description => string.Join(", ",
            Artists.Select(z => z.Name));

        public TrackType TrackType => TrackType.Track;
        public int Index => Number;
        public AlbumType AlbumType { get; set; }
        public bool IsDownloaded { get; set; }
    }
}
