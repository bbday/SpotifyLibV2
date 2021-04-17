﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpotifyLibrary.Interfaces;
internal delegate void HttpMessageReceived(HttpResponseMessage message);

namespace SpotifyLibrary.Helpers
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
            return response;
        }
    }
}