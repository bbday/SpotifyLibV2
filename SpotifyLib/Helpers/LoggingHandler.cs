using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyLib.Helpers
{
    internal class LoggingHandler : DelegatingHandler
    {
        private readonly SpotifyConnectionState _spotifyClient;

        internal LoggingHandler(HttpClientHandler innerHandler, SpotifyConnectionState connection) : base(innerHandler)
        {
            _spotifyClient = connection;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _spotifyClient.GetToken(cancellationToken);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    token.AccessToken);

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}