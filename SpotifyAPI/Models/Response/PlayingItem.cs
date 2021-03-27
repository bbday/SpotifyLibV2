using System;
using System.Collections.Generic;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Models.Response
{
    public class PlayingItem 
    {
        public PlayingItem(IAudioItem audioItem, 
            RepeatState repeatState,
            bool isShuffle,
            bool isPaused,
            IRemoteDevice activeDevice,
            IAudioId context, long timeStamp, long positionAsOfTimestamp, List<Descriptions> descriptions, TimeSpan duration)
        {
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
        public IAudioItem AudioItem { get; internal set; }
        public IAudioId Context { get; internal set; }
        public RepeatState RepeatState { get; set; }
        public bool IsShuffle { get; set; }
        public bool IsPaused { get; set; }
        public long TimeStamp
        {
            get
            {
                int diff = (int)(TimeProvider.CurrentTimeMillis() - _timeStamp);
                return (int)(_positionAsOfTimestamp + diff);
            }
        }
        public IRemoteDevice ActiveDevice { get; }


        private long _timeStamp { get; }
        private long _positionAsOfTimestamp { get; }
    }

    public class Descriptions
    {
        public Descriptions(string name, IAudioId id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public IAudioId Id { get; }
    }
}
