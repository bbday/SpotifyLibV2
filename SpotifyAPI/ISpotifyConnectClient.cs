using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Connectstate;
using JetBrains.Annotations;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Request.Playback;
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
        event EventHandler<List<IQueueUpdateItem>> QueueUpdated;
        ISpotifyPlayer Player { get; }
        PlayingItem LastReceivedCluster { get; }
        ManualResetEvent WaitForConnectionId { get; }
        string CurrentDevice { get; }

        Task<AcknowledgedResponse> InvokeCommandOnRemoteDevice(RemoteCommand playbackState,
            [CanBeNull] string deviceId = null);

        Task<AcknowledgedResponse> PlayItem(string connectClientCurrentDevice,
            IPlayRequest  request);

        Task<AcknowledgedResponse> Seek(double delta, string deviceId = null);
    }

    public enum RemoteCommand
    {
        Play,
        Pause,
        Skip,
        Previous,
        ShuffleToggle,
        RepeatContext,
        RepeatTrack,
        RepeatOff
    }
}
