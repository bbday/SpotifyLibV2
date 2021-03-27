﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
using SpotifyLibrary.Api;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Audio.KeyStuff;
using SpotifyLibrary.Authentication;
using SpotifyLibrary.ClientHandlers;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.JsonConverters;
using SpotifyLibrary.Models;
using SpotifyLibrary.Player;
using SpotifyLibrary.Services;
using SpotifyLibrary.Services.Mercury;
using SpotifyLibrary.Services.Mercury.Interfaces;

namespace SpotifyLibrary
{
    public class SpotifyClient
    {
        private ISpotifyConnectClient _connectClient;
        private IMercuryClient _mercuryClient;
        private ITokensProvider _tokens;
        private ICdnManager _cdnManager;
        private IAudioKeyManager _audioKeyManager;
        private IPlayableContentFeeder _contentFeeder;
        private Task<IViewsClient> _viewsClient;
        private Task<IMeClient> _meClient;
        private Task<ITracksClient> _tracksClient;

        public SpotifyClient()
        {
        }


        public Task<SpotifyConnectionResult> Authenticate(SpotifyConfiguration config)
        {
            Config = config;
            return MercuryClient.Connection.Connect();
        }

        public Task AttachClient(ISpotifyConnectClient connectClient,
            IWebsocketClient ws)
        {
            var dealerClient = new DealerClient(Tokens, ws);
            dealerClient.Attach();
            connectClient.Client = this;
            connectClient.DealerClient = dealerClient;
            _connectClient = connectClient;
            return dealerClient.Connect();
        }
        public ISpotifyConnectClient ConnectClient
        {
            get
            {
                if (_connectClient == null)
                    throw new AccessViolationException("You should initialize the connect dealer client first..");
                return _connectClient;
            }
        }
        public ITokensProvider Tokens => _tokens ??= new TokensProvider(MercuryClient);

        public IMercuryClient MercuryClient
        {
            get
            {
                _mercuryClient ??=
                    new MercuryClient(this,
                        (at, endedAt, reason) => { ConnectionDropped?.Invoke(this, (at, endedAt, reason)); }, async timetaken =>
                        {
                            var user = await MeClient;
                            var curUser = await user.GetCurrentUser();
                            Authenticated?.Invoke(this, (timetaken, curUser));
                        }, _audioKeyManager);
                return _mercuryClient;
            }
        }

        public bool IsConnected => MercuryClient?.Connection != null
                                   && MercuryClient.Connection.IsConnected;

        public ConcurrentDictionary<string, string> UserAttributes => MercuryClient.UserAttributes;

        public SpotifyConfiguration Config
        {
            get; private set;
        }
        [CanBeNull] public ApiResponse LastResponse { get; private set; }

        public ICdnManager CdnManager
        {
            get
            {
                if (_cdnManager != null) return _cdnManager;
                _cdnManager = new CdnManager(this);
                return _cdnManager;
            }
        }

        [CanBeNull] public ISpotifyPlayer Player => ConnectClient?.Player;
        public IAudioKeyManager AudioKeyManager
        {
            get
            {
                if (_audioKeyManager != null) return _audioKeyManager;
                _audioKeyManager = new AudioKeyManager(MercuryClient);
                return _audioKeyManager;
            }
        }

        public IPlayableContentFeeder ContentFeeder
        {
            get
            {
                if (_contentFeeder != null) return _contentFeeder;
                _contentFeeder = new ContentFeeder(Tokens);
                return _contentFeeder;
            }
        }

        public Task<ITracksClient> TracksClient => _tracksClient ??= BuildLoggableClient<ITracksClient>();
        public Task <IMeClient> MeClient => _meClient ??= BuildLoggableClient<IMeClient>();
        public Task<IViewsClient> ViewsClient => 
            _viewsClient ??= BuildLoggableClient<IViewsClient>();

        public string CountryCode => MercuryClient.Connection.CountryCode;
        public ICacheManager CacheManager { get; set; }

        public event EventHandler<(DateTime StartedAt, DateTime EndedAt, ConnectionDroppedReason Reason)>
            ConnectionDropped;

        public event EventHandler<(TimeSpan TimeTook, IAudioUser User)>
            Authenticated;

        public event EventHandler<LoggedOutReason> LoggedOut; 
        private async Task<T> BuildLoggableClient<T>()
        {
            var type = typeof(T);
            var baseUrl = await ResolveBaseUrlFromAttribute(type);

            //var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
            //    .Or<TimeoutRejectedException>()
            //    .WaitAndRetryAsync(Config.RestRetryCont, Config.RetryTimeoutWaiter);
            //var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(Config.MaxTimeout);
            //var retryPolicyHandler = new PolicyHttpMessageHandler(retryPolicy);
            //var timeoutHandler = new PolicyHttpMessageHandler(timeoutPolicy);
            var handler = new LoggingHandler(new HttpClientHandler(),
                message => { LastResponse = new ApiResponse(message); }, Tokens);

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
                Converters = {new StringEnumConverter()}
            }));
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

    public enum LoggedOutReason
    {
        Manual
    }
}