using System;
using System.Threading;
using System.Threading.Tasks;
using Spotify.Lib.Models;
using Spotify.Lib.Models.Response;

namespace Spotify.Lib.Interfaces
{
    public interface ISpotifyConnectClient
    {
        ISpotifyPlayer Player { get; }
        PlayingItem LastReceivedCluster { get; }
        ManualResetEvent WaitForConnectionId { get; }
        string CurrentDevice { get; }
        event EventHandler<MediaPlaybackState> PlaybackStateChanged;
        event EventHandler<PlayingItem> NewPlaybackWrapper;
        event EventHandler<bool> ShuffleStateChanged;
        event EventHandler<double> PositionChanged;
        event EventHandler<RepeatState> RepeatStateChanged;

        Task<AcknowledgedResponse> InvokeCommandOnRemoteDevice(RemoteCommand playbackState,
            [CanBeNull] string deviceId = null);

        Task<AcknowledgedResponse> PlayItem(string connectClientCurrentDevice,
            IPlayRequest request);

        Task<AcknowledgedResponse> Seek(double delta, string deviceId = null);
    }
}