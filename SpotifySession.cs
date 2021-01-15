using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Spotify;
using SpotifyLibV2.Api;
using SpotifyLibV2.Authentication;
using SpotifyLibV2.Config;
using SpotifyLibV2.Crypto;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2
{
    public class SpotifySession : ISpotifySessionListener, ISpotifySession
    {
        public static async Task<SpotifySession> CreateAsync(
            IAuthenticator authenticator,
            SpotifyConfiguration config)
        {
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
        public ISpotifyApiClient SpotifyApiClient { get; }
        public ISpotifyReceiver SpotifyReceiver { get; }
        public string CountryCode { get; set; }

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
