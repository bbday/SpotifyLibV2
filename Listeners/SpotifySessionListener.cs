using Spotify;
using SpotifyLibV2.Interfaces;

namespace SpotifyLibV2.Listeners
{
    public interface ISpotifySessionListener
    {
        void Register();
        void Unregister();
        void ApWelcomeReceived(APWelcome apWelcome);
        void ApLoginFailedReceived(APLoginFailed apLoginFailed);
        void CountryCodeReceived(string countryCode);
    }
}
