using System.Collections.Generic;
using System.Threading.Tasks;
using Spotify;
using SpotifyLibV2.Abstractions;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect;
using SpotifyLibV2.Connect.Interfaces;

namespace SpotifyLibV2.Interfaces
{
    public interface ISpotifySession
    {
        APWelcome ApWelcome { get; set; }
        APLoginFailed ApLoginFailed { get; set; }
        SpotifyConfiguration Configuration { get; }
        ISpotifyApiClient SpotifyApiClient { get; }
        ISpotifyReceiver SpotifyReceiver { get; }
        ISpotifyConnectClient SpotifyConnectClient { get; }
        List<ISocialPresence> SocialPresenceListeners { get; }
        string CountryCode { get; }
        void AttachSocialPresence(ISocialPresence socialpresence);

        ISpotifyConnectClient AttachClient(ISpotifyConnectReceiver connectInterface,
            ISpotifyPlayer player,
            WebsocketHandler handler);
    }
}
