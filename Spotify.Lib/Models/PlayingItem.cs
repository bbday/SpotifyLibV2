using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.TracksnEpisodes;

namespace Spotify.Lib.Models
{
    public class PlayingItem
    {
        public PlayingItem(ISpotifyId audioItemId,
            RepeatState repeatState,
            bool isShuffle,
            bool isPaused,
            IRemoteDevice activeDevice,
            ISpotifyId context, long timeStamp, long positionAsOfTimestamp, List<IRemoteDevice> devices, RepeatedField<global::Connectstate.ProvidedTrack> inQueue)
        {
            AudioItemId = audioItemId;
            RepeatState = repeatState;
            IsShuffle = isShuffle;
            IsPaused = isPaused;
            ActiveDevice = activeDevice;
            Context = context;

            _timeStamp = timeStamp;
            _positionAsOfTimestamp = positionAsOfTimestamp;
            Devices = devices;
            InQueue = inQueue;
        }
        public RepeatedField<global::Connectstate.ProvidedTrack> InQueue { get; }
        public ISpotifyId AudioItemId { get; }
        public ISpotifyId Context { get; }
        public RepeatState RepeatState { get; internal set; }
        public bool IsShuffle { get; internal set; }
        public bool IsPaused { get; internal set; }

        public long TimeStamp
        {
            get
            {
                var diff = (int) (TimeProvider.CurrentTimeMillis() - _timeStamp);
                return (int) (_positionAsOfTimestamp + diff);
            }
        }

        public IRemoteDevice ActiveDevice { get; }


        private long _timeStamp { get; }
        private long _positionAsOfTimestamp { get; }
        public List<IRemoteDevice> Devices { get; }
    }
}