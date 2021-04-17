using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Models
{
    public class PlayingItem
    {
        public PlayingItem(ITrackItem audioItem,
            IAudioId group,
            RepeatState repeatState,
            bool isShuffle,
            bool isPaused,
            IRemoteDevice activeDevice,
            IAudioId context, long timeStamp, long positionAsOfTimestamp, List<Descriptions> descriptions,
            TimeSpan duration)
        {
            Group = group;
            AudioItem = audioItem;
            RepeatState = repeatState;
            IsShuffle = isShuffle;
            IsPaused = isPaused;
            ActiveDevice = activeDevice;
            Context = context;

            _timeStamp = timeStamp;
            _positionAsOfTimestamp = positionAsOfTimestamp;
            Descriptions = descriptions;
            Duration = duration;
        }

        public TimeSpan Duration { get; internal set; }
        public List<Descriptions> Descriptions { get; internal set; }
        public ITrackItem AudioItem { get; internal set; }
        public IAudioId Context { get; internal set; }
        public IAudioId Group { get; }
        public RepeatState RepeatState { get; set; }
        public bool IsShuffle { get; set; }
        public bool IsPaused { get; set; }

        public long TimeStamp
        {
            get
            {
                int diff = (int) (TimeProvider.CurrentTimeMillis() - _timeStamp);
                return (int) (_positionAsOfTimestamp + diff);
            }
        }

        public IRemoteDevice ActiveDevice { get; }


        private long _timeStamp { get; }
        private long _positionAsOfTimestamp { get; }
    }
}