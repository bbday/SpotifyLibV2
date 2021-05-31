using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connectstate;
using Google.Protobuf.Collections;
using Spotify.Lib.Models;
using Spotify.Lib.Models.Response;

namespace Spotify.Lib.Interfaces
{
    public interface ISpotifyConnectReceiver
    {
        IRemoteDevice ActiveDevice { get; }
        PlayingItem LastReceivedCluster { get; }
        bool IsPlayingOnRemoteDevice { get; }
        event EventHandler<IRemoteDevice> ActiveDeviceChanged;
        event EventHandler<RepeatState> RepeatStateChanged;
        event EventHandler<double> PositionChanged;
        event EventHandler<bool> IsShuffleCHanged;
        event EventHandler<bool> IsPausedChanged;
        event EventHandler<RepeatedField<ProvidedTrack>> QueueUpdated;
        event EventHandler<(PlayingItem Item, IRemoteDevice Device)> NewItem;
        event EventHandler<List<IRemoteDevice>> DevicesUpdated;
        event EventHandler IncomingTransfer;
        event EventHandler<Exception?> TransferDone;
        Task<AcknowledgedResponse> TransferDevice(string newDeviceId);

        Task<AcknowledgedResponse> InvokeCommandOnRemoteDevice(RemoteCommand playbackState,
            string? deviceId = null);

        Task<AcknowledgedResponse> Seek(double delta,
            string? deviceId = null);

        Task<AcknowledgedResponse> PlayItem(string connectClientCurrentDevice,
            IPlayRequest request);

        void AttachPlayer(ISpotifyPlayer player);
    }
}