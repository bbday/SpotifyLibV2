using System;
using System.Threading;
using Connectstate;
using SpotifyLibrary.Connect.Enums;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Player;

namespace SpotifyLibrary
{
    public interface ISpotifyConnectClient
    {
        DealerClient DealerClient { get; set; }
        SpotifyClient Client { get; set; }
        event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        event EventHandler<PlayingItem> NewPlaybackWrapper;
        event EventHandler<bool> ShuffleStateChanged;
        event EventHandler<double> PositionChanged;
        event EventHandler<RepeatState> RepeatStateChanged;
        ISpotifyPlayer Player { get; }
        PlayingItem LastReceivedCluster { get; }
        ManualResetEvent WaitForConnectionId { get; }
    }
}
