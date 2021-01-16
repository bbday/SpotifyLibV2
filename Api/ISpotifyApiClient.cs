using System;
using System.Collections.Generic;
using System.Text;
using Nito.AsyncEx;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Api
{
    public interface ISpotifyApiClient
    {
        AsyncLazy<IHomeClient> Home { get; }
        ITokensProvider Tokens { get; }
        IEventsService EventsService { get; }
        IMercuryClient MercuryClient { get; }
        AsyncLazy<ILibrary> Library { get; }
    }
}
