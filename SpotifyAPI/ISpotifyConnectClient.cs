using System;
using System.Threading;
using Connectstate;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Player;

namespace SpotifyLibrary
{
    public interface ISpotifyConnectClient
    {
        DealerClient DealerClient { get; set; }
        SpotifyClient Client { get; set; }

        event EventHandler<PlayingItem> NewPlaybackWrapper;
        ISpotifyPlayer Player { get; }
        PlayingItem LastReceivedCluster { get; }
        ManualResetEvent WaitForConnectionId { get; }
    }
}
