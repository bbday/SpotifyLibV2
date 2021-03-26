using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SpotifyLibrary.Services.Mercury.Interfaces;

namespace SpotifyLibrary.ClientHandlers
{
    internal class LoggingHandler : DelegatingHandler
    {
        private readonly HttpMessageReceived _messageReceivedDelegate;
        private readonly ITokensProvider _tokensProvider;

        internal LoggingHandler(HttpClientHandler innerHandler,
            HttpMessageReceived messageReceived,
            ITokensProvider tokensProvider) : base(innerHandler)
        {
            _messageReceivedDelegate = messageReceived;
            _tokensProvider = tokensProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer",
                    (await _tokensProvider.GetToken("playlist-read")).AccessToken);
            var response = await base.SendAsync(request, cancellationToken);
            _messageReceivedDelegate(response);
            Debug.WriteLine("Response:");
            Debug.WriteLine(response.ToString());
            if (response.Content != null) Debug.WriteLine(await response.Content.ReadAsStringAsync());

            Console.WriteLine();

            return response;
        }
    }
}