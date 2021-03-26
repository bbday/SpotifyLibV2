using System;
using Connectstate;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Models;
using SpotifyLibrary.Player;

namespace SpotifyLibrary
{
    public interface ISpotifyConnectClient
    {
        DealerClient DealerClient { get; set; }
        SpotifyClient Client { get; set; }

        event EventHandler<PlaybackItemWrapper> NewPlaybackWrapper;
        ISpotifyPlayer Player { get; }
    }
}
