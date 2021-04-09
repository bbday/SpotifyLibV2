using System;
using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using SpotifyLibrary.Sql.DBModels;

namespace SpotifyLibrary.Models.Playlists
{
    public class ChartTrack : IPlaylistTrack
    {

        public ChartTrack(DbTrack other, int v, DateTime addedAt, string addedBy, TrackType chartTrack)
        {
        }

        public AudioService AudioService { get; }
        public IAudioId Id { get; }
        public AudioType Type { get; }
        public List<UrlImage> Images { get; }
        public string Name { get; }
        public string Description { get; }
        public TrackType TrackType { get; }
        public TimeSpan? DurationTs { get; }
        public IAudioItem Group { get; }
        public long? Playcount { get; }
        public List<IAudioItem> Artists { get; }
        public int Index { get; }
        public DateTime AddedOn { get; }
        public string AddedBy { get; }
        public bool IsDownloaded { get; set; }
    }
}
