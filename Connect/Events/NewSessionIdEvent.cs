using SpotifyLibV2.Api;

namespace SpotifyLibV2.Connect.Events
{
    internal class NewSessionIdEvent : IGenericEvent
    {
        private readonly string _sessionId;
        private readonly LocalStateWrapper _localStateWrapper;
        internal NewSessionIdEvent(string sessionId, LocalStateWrapper localStateWrapper)
        {
            _sessionId = sessionId;
            _localStateWrapper = localStateWrapper;
        }
        public EventBuilder BuildEvent()
        {
           var newBuilder = new EventBuilder(EventType.NEW_SESSION_ID);
           newBuilder.Append(_sessionId);
           newBuilder.Append(_localStateWrapper.Context.Uri);
           newBuilder.Append(_localStateWrapper.Context.Uri);
           newBuilder.Append(TimeProvider.CurrentTimeMillis().ToString());
           newBuilder.Append("");
           newBuilder.Append(_localStateWrapper.ContextSize.ToString());
           newBuilder.Append(_localStateWrapper.ContextUrl);
           return newBuilder;
        }
    }
}
