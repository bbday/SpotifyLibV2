using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Org.BouncyCastle.Asn1.Ocsp;
using Refit;
using SpotifyLib.Api;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Api
{
    public class SpotifyApiClient : ISpotifyApiClient
    {
        public SpotifyApiClient(IMercuryClient mercuryClient)
        {
            MercuryClient = mercuryClient;
            Tokens = new TokenProvider(mercuryClient);
            EventsService = new EventsService(mercuryClient);

            Home = new AsyncLazy<IHomeClient>((async () => 
                await CreateAndRegister<IHomeClient>()));
            Library = new AsyncLazy<ILibrary>((async () =>
                await CreateAndRegister<ILibrary>()));
            PlayerClient = new AsyncLazy<IPlayerClient>((async () =>
                await CreateAndRegister<IPlayerClient>()));
            Tracks = new AsyncLazy<ITrack>((async () =>
                await CreateAndRegister<ITrack>()));
            
            Episodes = new AsyncLazy<IEpisodes>((async () =>
                await CreateAndRegister<IEpisodes>()));
            PathFinder = new AsyncLazy<IPathFinder>(
                async () => await CreateAndRegister<IPathFinder>());

            Album = new AsyncLazy<IAlbum>((async () =>
                await CreateAndRegister<IAlbum>()));
            Artist = new AsyncLazy<IArtist>(
                async () => await CreateAndRegister<IArtist>());
            Playlist = new AsyncLazy<IPlaylist>((async () =>
                await CreateAndRegister<IPlaylist>()));
            User = new AsyncLazy<IUserService>(
                async () => await CreateAndRegister<IUserService>());
            ConnectApi = new AsyncLazy<IConnectState>(async () => 
                await CreateAndRegister<IConnectState>());

            Metadata = new AsyncLazy<IMetadata>(async () =>
                await CreateAndRegister<IMetadata>());

            Me = new AsyncLazy<IMeClient>(async () =>
                await CreateAndRegister<IMeClient>());

            LicenseService = new AsyncLazy<IPlaybackLicense>(async () =>
                await CreateAndRegister<IPlaybackLicense>());

            SeekTables = new AsyncLazy<ISeektables>(async () =>
                await CreateAndRegister<ISeektables>());

            StorageResolve = new AsyncLazy<IStorageResolveService>(async () =>
                await CreateAndRegister<IStorageResolveService>());
        }

        public AsyncLazy<IHomeClient> Home { get; }
        public IEventsService EventsService { get; }
        public IMercuryClient MercuryClient { get; }
        public AsyncLazy<IStorageResolveService> StorageResolve { get; }
        public AsyncLazy<IPlaybackLicense> LicenseService { get; }
        public AsyncLazy<ISeektables> SeekTables { get; }
        public AsyncLazy<IPathFinder> PathFinder { get; }
        public AsyncLazy<IPlayerClient> PlayerClient { get; }
        public AsyncLazy<ITrack> Tracks { get; }
        public AsyncLazy<IEpisodes> Episodes { get; }
        public AsyncLazy<IConnectState> ConnectApi { get; }
        public AsyncLazy<ILibrary> Library { get; }
        public AsyncLazy<IAlbum> Album { get; }
        public AsyncLazy<IArtist> Artist { get; }
        public AsyncLazy<IPlaylist> Playlist { get; }
        public AsyncLazy<IUserService> User { get; }
        public AsyncLazy<IMetadata> Metadata { get; }
        public AsyncLazy<IMeClient> Me { get; }

        /// <summary>
        /// Way to fetch token. Rn the supported scope is "playlist-read" which provides access to all endpoints.
        /// See <see cref="TokenProvider"/>
        /// </summary>
        public ITokensProvider Tokens { get; }

        private async Task<T1> CreateAndRegister<T1>()
        {
            var type = typeof(T1);
            var baseUrl = new Uri(await ResolveBaseUrlFromAttribute(type));

            var c = new System.Net.Http.HttpClient(new AuthenticatedHttpClientHandler(()
                => Tokens.GetToken("playlist-read")?.AccessToken))
            {
                BaseAddress = baseUrl
            };
            c.DefaultRequestHeaders.Add("Accept", "application/json");

            
            if (type == typeof(IHomeClient) 
                || type == typeof(IMetadata)
                || type == typeof(IMeClient)
                || type == typeof(IConnectState))
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true, IgnoreNullValues = true
                };

                var settings = new RefitSettings()
                {
                    ContentSerializer = new SystemTextJsonContentSerializer(options)
                };
                var createdService = RestService.For<T1>(c, settings);
                return createdService;
            }
            else
            {
                var createdService = RestService.For<T1>(c);
                //ServiceLocator.Instance.Register(createdService);

                return createdService;
            }
        }

        private static async Task<string> ResolveBaseUrlFromAttribute(MemberInfo type)
        {
            var attribute = Attribute.GetCustomAttributes(type);

            if (attribute.FirstOrDefault(x => x is BaseUrlAttribute) is BaseUrlAttribute baseUrlAttribute)
            {
                return baseUrlAttribute.BaseUrl;
            }

            if (attribute.Any(x => x is ResolvedDealerEndpoint))
            {
                return await ApResolver.GetClosestDealerAsync();
            }

            if (attribute.Any(x => x is ResolvedSpClientEndpoint))
            {
                return await ApResolver.GetClosestSpClient();
            }

            throw new NotImplementedException("No BaseUrl or ResolvedEndpoint attribute was defined");
        }
    }
}
