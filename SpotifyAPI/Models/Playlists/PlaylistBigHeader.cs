using System.Collections.Generic;
using MusicLibrary.Interfaces;

namespace SpotifyLibrary.Models.Playlists
{
    public class PlaylistBigHeader : IPlaylistHeader
    {
        public PlaylistBigHeader(
            IAudioId id,
            string plistTitle,
            string plistDescription,
            string coverString,
            string bigCoverString,
            string topString,
            IEnumerable<object> caption, int followersCount)
        {
            Id = id;
            Title = plistTitle;
            Description = plistDescription;
            BigCoverString = bigCoverString;
            FollowersCount = followersCount;
            MainCoverString = coverString;
            TopString = topString.ToLowerInvariant();
            HorizontalCaptionItems = new List<object>(caption ?? new List<object>(0));
        }

        public IAudioId Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TopString { get; set; }
        public ICollection<object> HorizontalCaptionItems { get; set; }
        public string BigCoverString { get; set; }
        public string MainCoverString { get; set; }
        public int FollowersCount { get; }
    }
}

