using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nito.AsyncEx;
using Refit;
using Spotify.Lib.ApiStuff;
using Spotify.Lib.Attributes;
using Spotify.Lib.Connect;
using Spotify.Lib.Connect.Audio;
using Spotify.Lib.Exceptions;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;
using SpotifyLibrary.Clients;

namespace Spotify.Lib
{
    public class SpotifyClient
    {
        private AsyncLazy<IViewsClient>? _views;
        private AsyncLazy<ITracksClient>? _tracks;
        private AsyncLazy<IEpisodesClient>? _episodes;
        //private AsyncLazy<ISearchClient>? _search;
        //private AsyncLazy<IMetadata>? _metadata;
        //private AsyncLazy<IPlaylistsClient>? _playlists;
        //private AsyncLazy<IUsersClient>? _users;
        private AsyncLazy<IConnectState>? _connect;
        private AsyncLazy<IArtistsClient>? _artists;
        private AsyncLazy<IAlbumsClient>? _albums;
        private AsyncLazy<IColorsClient> _colors;
        private AsyncLazy<IMetadataClient> _metadata;
        private AsyncLazy<IPlaylistsClient> _playlists;
        private AsyncLazy<IUsersClient> _users;

        public SpotifyClient()
        {
            Instance = this;
        }

        internal SpotifyConnector Connector { get; private set; }

        internal static SpotifyClient Instance { get; private set; }

        /// <summary>
        /// The main entry point of the client. Call this to create a new SpotifyClient instance.
        /// </summary>
        /// <param name="authenticate"></param>
        /// <returns></returns>
        /// <exception cref="IOException">TCP Connection failed</exception>
        /// <exception cref="MercuryCannotReceiveException">Unknown case. Should not be thrown.</exception>
        /// <exception cref="SpotifyConnectionException">Unknown case. Should not be thrown.</exception>
        /// <exception cref="SpotifyAuthenticatedException">Bad Credentials etc.</exception>
        public async Task Authenticate(IAuthenticator authenticate, string locale = "en")
        {
            SpotifyConfig.Locale = locale;
            Tokens = new TokensClient();
            var connector = await SpotifyConnector.Connect(authenticate);
            Connector = connector;
            KeyManager = new AudioKeyManager();
        }
        public AsyncLazy<IViewsClient> Views =>
            _views ??= new AsyncLazy<IViewsClient>(BuildLoggableClient<IViewsClient>);

        public AsyncLazy<IConnectState> ConnectState => _connect
            ??= new AsyncLazy<IConnectState>(BuildLoggableClient<IConnectState>);
        public AsyncLazy<ITracksClient> TracksClient =>
            _tracks ??= new AsyncLazy<ITracksClient>(BuildLoggableClient<ITracksClient>);
        public AsyncLazy<IEpisodesClient> EpisodesClient =>
            _episodes ??= new AsyncLazy<IEpisodesClient>(BuildLoggableClient<IEpisodesClient>);
        public AsyncLazy<IArtistsClient> ArtistsClient =>
            _artists ??= new AsyncLazy<IArtistsClient>(BuildLoggableClient<IArtistsClient>);
        public AsyncLazy<IAlbumsClient> AlbumsClient =>
            _albums ??= new AsyncLazy<IAlbumsClient>(BuildLoggableClient<IAlbumsClient>);
        public AsyncLazy<IColorsClient> ColorsClient =>
            _colors ??= new AsyncLazy<IColorsClient>(BuildLoggableClient<IColorsClient>);
        public AsyncLazy<IMetadataClient> Metadata =>
            _metadata ??= new AsyncLazy<IMetadataClient>(BuildLoggableClient<IMetadataClient>);
        public AsyncLazy<IPlaylistsClient> PlaylistsClient =>
            _playlists ??= new AsyncLazy<IPlaylistsClient>(BuildLoggableClient<IPlaylistsClient>);
        public AsyncLazy<IUsersClient> UsersClient =>
            _users ??= new AsyncLazy<IUsersClient>(BuildLoggableClient<IUsersClient>);
        public TokensClient Tokens { get; private set; }
        public string Country { get; internal set; }

        public ConcurrentDictionary<string, string> UserAttributes { get; } =
            new ConcurrentDictionary<string, string>();

        public event EventHandler<StoredCredentials> ConnectionInstantiated;
        public event EventHandler<ConnectionDroppedReason> ConnectionDropped;
        public async Task<T> SendAsyncReturnJson<T>(RawMercuryRequest request, CancellationToken? ct, bool usenewtonsoft = false)
        {
            var newJsonMercuryRequest = new SystemTextJsonMercuryRequest<T>(request, usenewtonsoft);
            CancellationTokenSource cts = null;
            if (ct == null)
            {
                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                ct = cts.Token;
            }
            using var callback = new SyncCallback(ct.Value);

            var seq = await Connector.SendRequest(request, callback, ct.Value);
            if (seq != int.MinValue)
            {
                var resp = callback.WaitResponse();
                if (resp.Payload != null)
                {
                    cts?.Dispose();
                    return newJsonMercuryRequest.Instantiate(resp);
                }
            }
            cts?.Dispose();
            throw new MercuryCannotReceiveException(
                $"Request timeout out passed, yet no response. seq: {seq}");
        }
        public async Task<MercuryResponse> SendAsync(RawMercuryRequest request, CancellationToken? ct)
        {
            CancellationTokenSource cts = null;
            if (ct == null)
            {
                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                ct = cts.Token;
            }
            using var callback = new SyncCallback(ct.Value);

            var seq = await Connector.SendRequest(request, callback, ct.Value);
            if (seq != int.MinValue)
            {
                var resp = callback.WaitResponse();
                if (resp.Payload != null)
                {
                    cts?.Dispose();
                    return resp;
                }
            }
            cts?.Dispose();
            throw new MercuryCannotReceiveException(
                $"Request timeout out passed, yet no response. seq: {seq}");
        }
        internal virtual void OnConnectionDropped(ConnectionDroppedReason e)
        {
            ConnectionDropped?.Invoke(this, e);
        }
        internal virtual void OnConnectionInstantiated(StoredCredentials e)
        {
            ConnectionInstantiated?.Invoke(this, e);
        }

        private async Task<T> BuildLoggableClient<T>()
        {
            var type = typeof(T);
            var baseUrl = await ResolveBaseUrlFromAttribute(type);

            var handler = new LoggingHandler(new HttpClientHandler(), Tokens);
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

        public async Task<(ISpotifyConnectReceiver Receiver, PlayingItem InitialItem)> ConnectToRemote()
        {
            var recv = new SpotifyConnectReceiver();
            var connect = await Task.Run(async() => await recv.Connect());
            Receiver = recv;
            return (Receiver, connect);
        }
        public ISpotifyConnectReceiver Receiver { get; private set; }
        public string CannonicalUser => Connector?.ApWelcome?.CanonicalUsername;

        public AudioKeyManager KeyManager { get; private set; }
    }
}