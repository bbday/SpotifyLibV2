using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Playlists
{
    public class EmptyId : StandardIdEquatable<EmptyId>
    {

        public EmptyId(string uri, string id,
            AudioType type, AudioService service) 
            : base(uri, id, type, service)
        {
        }

        public override string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public override string ToHexId()
        {
            throw new NotImplementedException();
        }
    }
    public class UnknownTrack : IPlaylistTrack
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public UnknownTrack(int index, AudioService servce)
        {
            Type = AudioType.Track;
            Name = "Unknown Track";
            TrackType = TrackType.PlaylistTrack;
            AudioService = servce;
            DurationTs = TimeSpan.Zero;
            Playcount = 0L;
            AddedOn = DateTime.MinValue;
            AddedBy = "";
            Index = index;
            Description = "Unknown";
            Artists = new List<IAudioItem>(0);
            Group = new EmptyItem();
            Images = new List<UrlImage>(0);
            var randomString = RandomString(6);
            Id = new EmptyId(randomString, randomString, AudioType.Track, servce);
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
