using JetBrains.Annotations;
using SpotifyLibV2.Connect.Events;

namespace SpotifyLibV2.Api
{
    public interface IEventsService
    {
        void Language([NotNull] string lang);
        void SendEvent([NotNull] EventBuilder builder);
    }
}
