using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class ApolloStation
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titleUri")]
        public string TitleUri { get; set; }

        [JsonProperty("subtitles")]
        public List<object> Subtitles { get; set; }

        [JsonProperty("imageUri")]
        public string ImageUri { get; set; }

        [JsonProperty("seeds")]
        public List<string> Seeds { get; set; }

        [JsonProperty("tracks")]
        public List<ApolloTrack> Tracks { get; set; }

        [JsonProperty("related_artists")]
        public List<ApolloRelatedArtist> RelatedArtists { get; set; }

        [JsonProperty("next_page_url")]
        public string NextPageUrl { get; set; }
    }

    public class ApolloRelatedArtist
    {
        [JsonProperty("artistName")]
        public string artistName { get; set; }
        [JsonProperty("artistUri")]
        public string ArtistUri { get; set; }
    }
    public class ApolloTrack
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("artist_uri")]
        public string TrackArtistUri { get; set; }

        [JsonProperty("album_uri")]
        public string AlbumUri { get; set; }

        [JsonProperty("original_gid")]
        public string OriginalGid { get; set; }

        [JsonProperty("metadata")]
        public dynamic Metadata { get; set; }

        [JsonProperty("artistUri")]
        public string ArtistUri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
