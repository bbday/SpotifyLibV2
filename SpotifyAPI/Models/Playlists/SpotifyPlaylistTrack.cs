using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Sql;
using SpotifyLibrary.Sql.DBModels;

namespace SpotifyLibrary.Models.Playlists
{
    public class SpotifyPlaylistTrack : IPlaylistTrack, INotifyPropertyChanged
    {
        private readonly string _artistsString;
        private readonly string _groupString;
        private readonly string _uri;
        private IAudioId _id;
        private IAudioItem _group;
        private List<IAudioItem> _artists;
        private string _description;
        private bool _isDownloaded;

        public SpotifyPlaylistTrack(DbTrack dbTrack,
            int index,
            DateTime addedAt,
            string addedBy,
            TrackType playlistTrack,
            AudioService service)
        {
            _artistsString = dbTrack.ArtistsString;
            _groupString = dbTrack.GroupString;
            _uri = dbTrack.Uri;
            AddedOn = addedAt;
            AddedBy = addedBy;
            TrackType = playlistTrack;
            AudioService = service;
            DurationTs = TimeSpan.FromMilliseconds(dbTrack.DurationMs);
            Index = index;
            Name = dbTrack.Title;
        }

        public AudioService AudioService { get; }
        public IAudioId Id
        {
            get
            {
                if (_id != null) return _id;
                switch (AudioService)
                {
                    case AudioService.Spotify:
                        _id = new TrackId(_uri);
                        break;
                    case AudioService.Local:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return _id;
            }
        }
        public AudioType Type => AudioType.Track;

        public List<UrlImage> Images
        {
            get
            {
                if (_group != null) return _group.Images;
                switch (AudioService)
                {
                    case AudioService.Spotify:
                        _group = JsonConvert.DeserializeObject<IAudioItem>(_groupString, SqlDb.Settings);
                        break;
                    case AudioService.Local:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return _group.Images;
            }
        }
        public string Name { get; }
        public string Description => _description ??= string.Join(",", Artists.Select(z => z.Name));
        public int Index { get; } 
        public DateTime AddedOn { get; }
        public string AddedBy { get; }

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                _isDownloaded = value;
                OnPropertyChanged(nameof(IsDownloaded));
            }
        }
        public TrackType TrackType { get; }
        public TimeSpan? DurationTs { get; }
        public IAudioItem Group
        {
            get
            {
                if (_group != null) return _group;
                switch (AudioService)
                {
                    case AudioService.Spotify:
                        _group = JsonConvert.DeserializeObject<IAudioItem>(_groupString, SqlDb.Settings);
                        break;
                    case AudioService.Local:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return _group;
            }
        }
        public long? Playcount { get; }
        public List<IAudioItem> Artists
        {
            get
            {
                if (_artists != null) return _artists;
                switch (AudioService)
                {
                    case AudioService.Spotify:
                        _artists = JsonConvert.DeserializeObject<List<IAudioItem>>(_artistsString, SqlDb.Settings);
                        break;
                    case AudioService.Local:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return _artists;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
