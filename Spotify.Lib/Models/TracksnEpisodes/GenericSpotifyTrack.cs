using System.Collections.Generic;
using System.Text.Json.Serialization;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response.SpotItems;

namespace Spotify.Lib.Models.TracksnEpisodes
{
    public abstract class GenericSpotifyTrack : ISpotifyItem
    {
        private ISpotifyId _id;

        /// <summary>
        ///     If the track is explicit (contains vulgar language).
        /// </summary>
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }

        [Newtonsoft.Json.JsonIgnore] public ISpotifyId Id => _id ??= new TrackId(Uri);
        public virtual AudioItemType Type => AudioItemType.Track;

        /// <summary>
        ///     Name of the track.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        public string Uri { get; set; }

        public abstract string Caption { get; }
        public abstract List<UrlImage> Images { get; set; }
        public abstract string Description { get; set; }
    }
}