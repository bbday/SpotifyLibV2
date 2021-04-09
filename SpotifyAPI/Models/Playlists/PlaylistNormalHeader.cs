using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MusicLibrary.Interfaces;

namespace SpotifyLibrary.Models.Playlists
{
    public class PlaylistNormalHeader : IPlaylistHeader, INotifyPropertyChanged
    {
        private string _header;
        public PlaylistNormalHeader(
            IAudioId id,
            string plistTitle,
            string plistDescription,
            string coverString,
            string topString,
            ICollection<object> caption,
            int followerCount)
        {
            Id = id;
            Title = plistTitle;
            Description = plistDescription;
            MainCoverString = coverString;
            TopString = topString.ToUpperInvariant();
            HorizontalCaptionItems = new List<object>(caption ?? new List<object>());
            FollowerCount = followerCount;
        }
        public IAudioId Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TopString { get; set; }
        public ICollection<object> HorizontalCaptionItems { get; set; }
        public string MainCoverString { get; set; }

        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        public int FollowerCount { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

