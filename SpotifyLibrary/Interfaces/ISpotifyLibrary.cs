using System;
using System.Threading;
using System.Threading.Tasks;
using MediaLibrary.Interfaces;
using Nito.AsyncEx;
using SpotifyLibrary.Api;
using SpotifyLibrary.Clients;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface ISpotifyLibrary
    {
        /// <summary>
        /// Will be invoked when a connection just got dropped
        /// </summary>
        event EventHandler<DroppedReason> ConnectionDropped;

        /// <summary>
        /// Will be invoked when a connection is about to occur
        /// </summary>
        event EventHandler ConnectionStarted;
        /// <summary>
        /// Will always be invoked, even when an error occured during connection.
        /// </summary>
        event EventHandler<ApWelcomeOrFailed> ConnectionDone;

        /// <summary>
        /// Will always be invoked, even when an error occured during connection.
        /// </summary>
        event EventHandler<Exception> ConnectionError;

        /// <summary>
        /// Default authenticator to use. Can be changed during runtime but will require a reconnect (Will be scheduled)
        /// Register to the <see cref="ConnectionStarted"/> and <see cref="ConnectionDone"/> events for info.
        /// </summary>
        IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Configuration used throughout the app.
        /// See <see cref="SpotifyConfiguration"/> to look at which fields can be changed during runtime.
        /// </summary>
        SpotifyConfiguration Configuration { get; }


        /// <summary>
        /// Client to communicate with the hm:// endpoints. See <see cref="IMercuryClient"/> for methods.
        /// The user should not invoke this themselves, but the libarary will internall call the required methods based on usage.
        /// </summary>
        IMercuryClient MercuryClient { get; }
        IAudioUser CurrentUser { get; }
        ITokensProvider Tokens { get; }
        ISpotifyConnectReceiver ConnectReceiver { get; }
        IAudioKeyManager KeyManager { get; }
        AsyncLazy<IViewsClient> Views { get; }
        AsyncLazy<ITracksClient> TracksClient { get;  }
        AsyncLazy<IEpisodesClient> EpisodesClient { get; }
        AsyncLazy<ISearchClient> SearchClient { get; }
        AsyncLazy<IMetadata> MetadataClient { get; }
        AsyncLazy<IPlaylistsClient> PlaylistsClient { get; }
        AsyncLazy<IUsersClient> UserClient { get; }
        AsyncLazy<IConnectState> ConnectState { get; }
        AsyncLazy<IArtistsClient> ArtistsClient { get; }
        Task<ApWelcomeOrFailed> Authenticate(IAuthenticator authenticator, CancellationToken ct);

        Task<(ISpotifyConnectReceiver Receiver, PlayingItem InitialItem)> ConnectToRemote(IWebsocketClient websocket,
            ISpotifyPlayer player);
    }
}
