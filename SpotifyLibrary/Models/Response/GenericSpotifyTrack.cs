using System.Collections.Generic;
using System.Text.Json.Serialization;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response
{
    public class GenericSpotifyTrack : ISpotifyItem
    {
        protected IAudioId _id;

        public AudioServiceType AudioService => AudioServiceType.Spotify;
        [Newtonsoft.Json.JsonIgnore]
        public virtual IAudioId Id => _id ??= new TrackId(Uri);
        public virtual AudioItemType Type => AudioItemType.Track;

        /// <summary>
        /// Name of the track.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// If the track is explicit (contains vulgar language).
        /// </summary>
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }

        public string Uri { get; set; }

        public virtual List<UrlImage> Images { get; set; }
        public virtual string Description { get; set; }
    }
}
