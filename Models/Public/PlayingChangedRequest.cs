using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Models.Public
{
    public readonly struct PlayingChangedRequest
    {
        public PlayingChangedRequest(
            RepeatState repeatState,
            bool isShuffle, 
            string itemUri, 
            string contextUri, 
            bool? isPaused,
            bool? isPlaying,
            long? timeStamp)
        {
            RepeatState = repeatState;
            IsShuffle = isShuffle;
            ItemUri = itemUri;
            ContextUri = contextUri;
            IsPaused = isPaused;
            IsPlaying = isPlaying;
            TimeStamp = timeStamp;
        }

        public RepeatState RepeatState { get; }
        public bool IsShuffle { get; }
        public string ItemUri { get; }
        public string ContextUri { get;  }
        public bool? IsPaused { get;  }
        public bool? IsPlaying { get;  }
        public long? TimeStamp { get; }
    }
}

