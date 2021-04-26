using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Interfaces
{
    public interface ISpotifyConnectReceiver
    {
        event EventHandler<IRemoteDevice> ActiveDeviceChanged;
        event EventHandler<RepeatState> RepeatStateChanged;
        event EventHandler<double> PositionChanged;
        event EventHandler<bool> IsShuffleCHanged;
        event EventHandler<bool> IsPausedChanged;
        event EventHandler<(PlayingItem Item, IRemoteDevice Device)> NewItem;
        event EventHandler<List<IRemoteDevice>> DevicesUpdated;
        event EventHandler IncomingTransfer;
        event EventHandler<Exception?> TransferDone;
        IRemoteDevice ActiveDevice { get; }
        PlayingItem LastReceivedCluster { get; }
        Task<AcknowledgedResponse> TransferDevice(string newDeviceId);

        Task<AcknowledgedResponse> InvokeCommandOnRemoteDevice(RemoteCommand playbackState,
            string? deviceId = null);
        Task<AcknowledgedResponse> Seek(double delta,
            string? deviceId = null);

        Task<AcknowledgedResponse> PlayItem(string connectClientCurrentDevice,
            IPlayRequest request);
    }
}
