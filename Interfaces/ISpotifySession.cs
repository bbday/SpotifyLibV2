using Spotify;
using SpotifyLibV2.Abstractions;
using SpotifyLibV2.Api;
using SpotifyLibV2.Audio;
using SpotifyLibV2.Audio.Cdn;
using SpotifyLibV2.Audio.Decrypt;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Listeners;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

namespace SpotifyLibV2.Interfaces
{
    public interface ISpotifySession
    {
        ICacheManager CacheManager { get; }
        ISpotifyPlayer Player
        { 
            get; set;
        }

        IAudioKey AudioKeyManager { get; }
        IChannelManager ChannelManager { get; }
        ICdnManager CdnManager { get; }
        IPlayableContentFeeder ContentFeeder { get; }
        HttpClient MetadataClient { get; }
        APWelcome ApWelcome { get; set; }
        APLoginFailed ApLoginFailed { get; set; }
        SpotifyConfiguration Configuration { get; }
        ISpotifyApiClient SpotifyApiClient { get; }
        ISpotifyReceiver SpotifyReceiver { get; }
        ISpotifyConnectClient SpotifyConnectClient { get; }
        List<ISocialPresence> SocialPresenceListeners { get; }
        string CountryCode { get; }
        void AttachSocialPresence(ISocialPresence socialpresence);
        void AttachPlaylistListener(PlaylistId uri, IPlaylistListener listener);
        void SetCache(ICache cache);
        ISpotifyConnectClient AttachClient(ISpotifyConnectReceiver connectInterface,
            WebsocketHandler handler);
        ConcurrentDictionary<string, string> UserAttributes { get; }
    }
}
