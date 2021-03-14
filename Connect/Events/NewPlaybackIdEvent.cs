using SpotifyLibV2.Api;

namespace SpotifyLibV2.Connect.Events
{
    public readonly struct NewPlaybackIdEvent : IGenericEvent
    {
        private readonly string _sessionId;
        private readonly string _playbackId;

        public NewPlaybackIdEvent(string sessionId, string playbackId)
        {
            _sessionId = sessionId;
            _playbackId = playbackId;
        }


        public EventBuilder BuildEvent()
        {
            var @event = new EventBuilder(EventType.NEW_PLAYBACK_ID);
            @event.Append(_playbackId)
                .Append(_sessionId)
                .Append(TimeProvider.CurrentTimeMillis().ToString());
            return @event;
        }
    }
}
