using System;
using System.Collections.Generic;
using System.Text;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers.JsonConverters;

namespace SpotifyLibrary.Models.Response
{
    public class PathfinderAlbum
    {
        [JsonProperty("album")]
        public PathfinderAlbumData Album { get; set; }
    }
    public class PathfinderAlbumData
    {
        public PathfinderAlbumTracks Tracks { get;set;}
    }

    public class PathfinderAlbumTracks
    {
        public int TotalCount { get; set; }
        public List<PathfinderTrack> Items { get; set; }
    }

    public class PathfinderTrack
    {
        [JsonConverter(typeof(PathfinderTrackToAudioItemConverter))]
        public IAlbumTrack Track { get; set; }
        public string Uid { get; set; }
    }
}
