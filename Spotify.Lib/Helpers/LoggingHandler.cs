using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Spotify.Lib.Helpers
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly TokensClient _tokensProvider;

        public LoggingHandler(HttpClientHandler innerHandler, TokensClient tokensProvider) : base(innerHandler)
        {
            _tokensProvider = tokensProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    (await _tokensProvider.GetToken(cancellationToken, "playlist-read")).AccessToken);

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}