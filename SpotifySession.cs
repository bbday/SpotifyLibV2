using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Caching.Memory;
using Spotify;
using SpotifyLibV2.Abstractions;
using SpotifyLibV2.Api;
using SpotifyLibV2.Authentication;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Crypto;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Interfaces;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2
{
    public class SpotifySession : ISpotifySessionListener, ISpotifySession
    {
        public static IMemoryCache Cache;
        private ISpotifyConnectClient _spotifyConnectClient;

        public static async Task<SpotifySession> CreateAsync(
            IAuthenticator authenticator,
            SpotifyConfiguration config)
        {
            Cache = new MemoryCache(new MemoryCacheOptions());

            if (authenticator == null) 
                throw new ArgumentNullException(nameof(authenticator), "Authenticator cannot be null");

            var tcpClientTask = ApResolver.GetClosestAccessPoint()
                .ContinueWith(z => new SpotifyClient(z.Result.host, z.Result.port));

            var loginCredentialsTask = authenticator.Get();
            await Task.WhenAll(tcpClientTask, loginCredentialsTask);
            return new SpotifySession(await loginCredentialsTask, config, await tcpClientTask);
        }      
        
        private readonly DiffieHellman _keys;
        private SpotifySession(
            LoginCredentials credentials,
            SpotifyConfiguration config,
            SpotifyClient spotifyClient)
        {
            Configuration = config;
            ListenersHolder.SpotifySessionConcurrentDictionary.Add(this);
            _keys = new DiffieHellman();
            var clHello = NewClientHello();

            var connected = spotifyClient.GetStream()
                .ConnectToSpotify(clHello, _keys);
            if (!connected) throw new SpotifyConnectionException("Could not connect to spotify for unknown reasons..");

            spotifyClient.GetStream().Authenticate(credentials, config);

            var mercuryClient = new MercuryClient(spotifyClient.GetStream());

            SpotifyApiClient = new SpotifyApiClient(mercuryClient);
            SpotifyReceiver = new SpotifyReceiver(spotifyClient.GetStream(), mercuryClient, new CancellationToken());
        }

        private ClientHello NewClientHello()
        {
            var clientHello = new ClientHello
            {
                BuildInfo = new BuildInfo
                {
                    Platform = Platform.Win32X86,
                    Product = Product.Client,
                    ProductFlags = { ProductFlags.ProductFlagNone },
                    Version = 112800721
                }
            };
            clientHello.CryptosuitesSupported.Add(Cryptosuite.Shannon);
            clientHello.LoginCryptoHello = new LoginCryptoHelloUnion
            {
                DiffieHellman = new LoginCryptoDiffieHellmanHello
                {
                    Gc = ByteString.CopyFrom(_keys.PublicKeyArray()),
                    ServerKeysKnown = 1
                }
            };
            var nonce = new byte[16];
            (new Random()).NextBytes(nonce);
            clientHello.ClientNonce = ByteString.CopyFrom(nonce);
            clientHello.Padding = ByteString.CopyFrom(new byte[1]
            {
                (byte) 30
            });

            return clientHello;
        }
        public APWelcome ApWelcome { get; set; }
        public APLoginFailed ApLoginFailed { get; set; }
        public SpotifyConfiguration Configuration { get; }
        public ISpotifyApiClient SpotifyApiClient { get; }
        public ISpotifyReceiver SpotifyReceiver { get; }
        public ISpotifyConnectClient SpotifyConnectClient
        {
            get
            {
                if (_spotifyConnectClient == null)
                    throw new ArgumentNullException(nameof(SpotifyConnectClient),
                        "Please initialize the connect client first by attaching the client.");
                return _spotifyConnectClient;
            }
            private set => _spotifyConnectClient = value;
        }
        public string CountryCode { get; set; }

        public ISpotifyConnectClient AttachClient(
            ISpotifyConnectReceiver connectInterface,
            ISpotifyPlayer player,
            WebsocketHandler handler)
        {
            IDealerClient dealerClient = 
                new DealerClient(SpotifyApiClient.Tokens, handler);  
            dealerClient.Attach();
            ISpotifyConnectClient connectClient =
                new SpotifyConnectClient(dealerClient, player,
                    connectInterface, 
                    SpotifyApiClient.EventsService,
                    SpotifyApiClient.Tokens,
                    SpotifyApiClient.PlayerClient,
                    Configuration);
            SpotifyConnectClient = connectClient;
            return connectClient;
        }

        public void ApWelcomeReceived(APWelcome apWelcome)
        {
            ApWelcome = apWelcome;
        }

        public void ApLoginFailedReceived(APLoginFailed apLoginFailed)
        {
            ApLoginFailed = apLoginFailed;
        }

        public void CountryCodeReceived(string countryCode)
        {
            CountryCode = countryCode;
        }

        public void Register()
        {
            ListenersHolder.SpotifySessionConcurrentDictionary.Add(this);
        }
        public void Unregister()
        {
            ListenersHolder.SpotifySessionConcurrentDictionary.Remove(this);
        }
    }
}
