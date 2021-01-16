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
        AsyncLazy<IPlayerClient> PlayerClient { get; }
        AsyncLazy<ITrack> Tracks { get; }
        AsyncLazy<ILibrary> Library { get; }
    }
}
