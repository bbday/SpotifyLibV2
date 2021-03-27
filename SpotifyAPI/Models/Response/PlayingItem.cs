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
            IAudioId context, long timeStamp, long positionAsOfTimestamp)
        {
            AudioItem = audioItem;
            RepeatState = repeatState;
            IsShuffle = isShuffle;
            IsPaused = isPaused;
            ActiveDevice = activeDevice;
            Context = context;

            _timeStamp = timeStamp;
            _positionAsOfTimestamp = positionAsOfTimestamp;
        }

        public IAudioItem AudioItem { get; }
        public IAudioId Context { get; }
        public RepeatState RepeatState { get; }
        public bool IsShuffle { get; }
        public bool IsPaused { get; }
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
}
