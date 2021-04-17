using System;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nito.AsyncEx;
using Refit;
using SpotifyLibrary.Api;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Authenticators;
using SpotifyLibrary.Clients;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Connect;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Response.SpotifyItems;
using IAuthenticator = SpotifyLibrary.Interfaces.IAuthenticator;

namespace SpotifyLibrary
{
    /// <summary>
    /// The base class for all communication with spotify.
    /// </summary>
    public class SpotifyLibrary : ISpotifyLibrary, IMusicService
    {
        private IAuthenticator _authenticator;
        private AsyncLazy<IViewsClient>? _views;
        private AsyncLazy<ITracksClient>? _tracks;
        private AsyncLazy<IEpisodesClient>? _episodes;
        private AsyncLazy<ISearchClient>? _search;
        private AsyncLazy<IMetadata>? _metadata;
        private AsyncLazy<IPlaylistsClient>? _playlists;
        private AsyncLazy<IUsersClient>? _users;
        public IAudioUser? CurrentUser { get; private set; }
        public AudioServiceType ServiceType => AudioServiceType.Spotify;

        /// <summary>
        /// Creates a new instance of
        /// <see cref="SpotifyLibrary"/> 
        /// </summary>
        /// <param name="authenticator">An instance of either <para /> <see cref="UserPassAuthenticator"/></param>
        /// <param name="config">Configuration class</param>
#pragma warning disable 8618
        public SpotifyLibrary(
#pragma warning restore 8618
            SpotifyConfiguration config,
            IAuthenticator authenticator = null)
        {
            config ??= SpotifyConfiguration.Default();
            Configuration = config;
            Authenticator = authenticator;

            MercuryClient = new MercuryClient(this);
            Tokens = new TokensClient(this);

            NetworkChange.NetworkAvailabilityChanged += NetworkChangeOnNetworkAvailabilityChanged;
            Instance = this;
        }

        private void NetworkChangeOnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (!e.IsAvailable)
            {
                ConnectionDropped?.Invoke(this, DroppedReason.Network);
            }
        }

        /// <summary>
        /// Creates a new instance of
        /// <see cref="SpotifyLibrary"/> using a default instance
        /// of <see cref="SpotifyConfiguration"/>
        /// using: <seealso cref="SpotifyConfiguration.Default()"/>
        /// </summary>
        /// <param name="authenticator">An instance of either <para /> <see cref="UserPassAuthenticator"/></param>
        public SpotifyLibrary(IAuthenticator authenticator) : this(
            SpotifyConfiguration.Default(), authenticator)
        {

        }

        public event EventHandler<DroppedReason> ConnectionDropped;


        public event EventHandler ConnectionStarted;

        public event EventHandler<ApWelcomeOrFailed> ConnectionDone;

        public event EventHandler<Exception> ConnectionError;


        // ReSharper disable once ConvertToAutoProperty
        public IAuthenticator Authenticator
        {
            get => _authenticator;
            set
            {
                // ReSharper disable once ArrangeAccessorOwnerBody
                _authenticator = value;

                //TODO: Reauthenticate?
            }
        }


        public SpotifyConfiguration Configuration { get; }

        public IMercuryClient MercuryClient { get; private set; }
        public ITokensProvider Tokens { get; }
        public ISpotifyConnectReceiver ConnectReceiver { get; }

        public AsyncLazy<IViewsClient> Views =>
            _views ??= new AsyncLazy<IViewsClient>(BuildLoggableClient<IViewsClient>);

        public AsyncLazy<ITracksClient> TracksClient =>
            _tracks ??= new AsyncLazy<ITracksClient>(BuildLoggableClient<ITracksClient>);
        public AsyncLazy<IEpisodesClient> EpisodesClient =>
            _episodes ??= new AsyncLazy<IEpisodesClient>(BuildLoggableClient<IEpisodesClient>);
        public static string Country => Instance.MercuryClient.CountryCode;

        internal static SpotifyLibrary Instance { get; private set; }

        public AsyncLazy<ISearchClient> SearchClient =>
            _search ??= new AsyncLazy<ISearchClient>(BuildLoggableClient<ISearchClient>);

        public AsyncLazy<IMetadata> MetadataClient =>
            _metadata ??= new AsyncLazy<IMetadata>(BuildLoggableClient<IMetadata>);

        public AsyncLazy<IPlaylistsClient> PlaylistsClient =>
            _playlists ??= new AsyncLazy<IPlaylistsClient>(BuildLoggableClient<IPlaylistsClient>);

        public AsyncLazy<IUsersClient> UserClient  => _users 
            ??= new AsyncLazy<IUsersClient>(BuildLoggableClient<IUsersClient>);

        public Task<ApWelcomeOrFailed> Authenticate(IAuthenticator authenticator, CancellationToken ct)
        {
            Authenticator = authenticator;
            MercuryClient?.Dispose();
            MercuryClient = new MercuryClient(this);
            return Task.Run(() => MercuryClient.Connection.Connect(authenticator, ct), ct);
        }

        public async Task<(ISpotifyConnectReceiver Receiver, PlayingItem InitialItem)> ConnectToRemote(IWebsocketClient websocket)
        {
            var newConnectClient = new SpotifyConnectReceiver(this);
            CurrentUser ??= new PrivateUser
            {
                Uri = MercuryClient.Connection.WelcomeOrFailed.Value.Welcome.CanonicalUsername
            };
            return (newConnectClient, await newConnectClient.Connect(websocket));
        }

        internal virtual void OnConnectionError(Exception ex)
        {
            ConnectionError?.Invoke(this, ex);
        }

        internal virtual void OnConnectionDone(ApWelcomeOrFailed apWelcomeOrFailed)
        {
            ConnectionDone?.Invoke(this, apWelcomeOrFailed);
        }

        internal virtual void OnConnectionStarted()
        {
            ConnectionStarted?.Invoke(this, EventArgs.Empty);
        }

        internal virtual void OnConnectionDropped(Exception? exception)
        {
            if (exception != null)
            {
                ConnectionDropped?.Invoke(this, DroppedReason.Error);
                ConnectionError?.Invoke(this, exception);
            }
            else
            {
                ConnectionDropped?.Invoke(this, DroppedReason.Unknown);
            }
        }

        private async Task<T> BuildLoggableClient<T>()
        {
            var type = typeof(T);
            var baseUrl = await ResolveBaseUrlFromAttribute(type);

            var handler = new LoggingHandler(new HttpClientHandler(),
                message => {  }, Tokens);
            var client =
                new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl)
                };
            //services.AddTransient<LoggingHandler>(handler);
            //services.AddRefitClient<T>()
            //    .ConfigureHttpClient(httpClient =>
            //    {

            //    })
            //    .AddHttpMessageHandler(handler)
            //    .AddPolicyHandler(retryPolicy)
            //    .AddPolicyHandler(timeoutPolicy) // The order of adding is imporant!
            //    .AddHttpMessageHandler<AuthorizationMessageHandler>();

            var refitSettings = new RefitSettings(new Refit.NewtonsoftJsonContentSerializer(new JsonSerializerSettings()
            {
                ContractResolver = new CustomResolver(),
                Converters = { new StringEnumConverter() }
            }));
            //var refitSettings = new RefitSettings(new
            //    SystemTextJsonContentSerializer(new JsonSerializerOptions
            //    {
            //        //ContractResolver = new CustomResolver(),
            //        Converters =
            //        {
            //            new SpotifyItemConverterTextJson(),
            //            new JsonStringEnumConverter(),
            //        },
            //        PropertyNameCaseInsensitive = false
            //    }));
            var refitClient = RestService.For<T>(client, refitSettings);

            return refitClient;
        }
        private static async Task<string> ResolveBaseUrlFromAttribute(MemberInfo type)
        {
            var attribute = Attribute.GetCustomAttributes(type);

            if (attribute.FirstOrDefault(x => x is BaseUrlAttribute) is BaseUrlAttribute baseUrlAttribute)
                return baseUrlAttribute.BaseUrl;

            if (attribute.Any(x => x is ResolvedDealerEndpoint)) return await ApResolver.GetClosestDealerAsync();

            if (attribute.Any(x => x is ResolvedSpClientEndpoint)) return await ApResolver.GetClosestSpClient();

            if (attribute.Any(x => x is OpenUrlEndpoint)) return "https://api.spotify.com";

            throw new NotImplementedException("No BaseUrl or ResolvedEndpoint attribute was defined");
        }
    }
}
