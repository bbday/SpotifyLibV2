using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Connectstate;
using Extensions;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Nito.AsyncEx;
using Spotify.Playlist4.Proto;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyConnectReceiver : ISpotifyConnectReceiver
    {
        private readonly AsyncLock clusterLock = new AsyncLock();
        internal readonly ISpotifyLibrary Library;
        private ManualResetEvent _waitForConid = null!;
        internal SpotifyMessageState MessageState;
        internal SpotifyRequestState RequestState;
        private List<IRemoteDevice> _allDevices;
        private IRemoteDevice _activeDevice;

        internal SpotifyConnectReceiver(ISpotifyLibrary library)
        {
            Library = library;
        }

        internal async Task<PlayingItem> Connect(IWebsocketClient ws, ISpotifyPlayer player)
        {
            Player = player;

            var dealerClient = new DealerClient((Library.CurrentUser as PrivateUser)!
                .Uri.Split(':').Last(),
                Library.Tokens, ws);
            dealerClient.Attach();
            RequestState = new SpotifyRequestState(Library, dealerClient, this);

            MessageState = new SpotifyMessageState(dealerClient, RequestState, this);
            var connected = await dealerClient.Connect();
            WaitForConnectionId.WaitOne();
            return LastReceivedCluster;
        }
        internal ISpotifyPlayer Player { get; private set; }

        public event EventHandler<List<IRemoteDevice>>? DevicesUpdated;
        public event EventHandler? IncomingTransfer;
        public event EventHandler<Exception?>? TransferDone;

        public List<IRemoteDevice> AllDevices
        {
            get => _allDevices;
            private set
            {
                _allDevices = value;
                DevicesUpdated?.Invoke(this, value);
            }
        }

        public IRemoteDevice? ActiveDevice
        {
            get => _activeDevice;
            set
            {
                var update = !(value?.Equals(_activeDevice) ?? true);
                _activeDevice = value;
                if (update)
                {
                    ActiveDeviceChanged?.Invoke(this, value);
                }
            }
        } 
        public event EventHandler<IRemoteDevice> ActiveDeviceChanged;
        public event EventHandler<RepeatState>? RepeatStateChanged;
        public event EventHandler<double>? PositionChanged;
        public event EventHandler<bool>? IsShuffleCHanged;
        public event EventHandler<bool>? IsPausedChanged;
        public event EventHandler<(PlayingItem Item, IRemoteDevice Device)>? NewItem;
        internal ManualResetEvent WaitForConnectionId
        {
            get { return _waitForConid ??= new ManualResetEvent(false); }
        }

        public PlayingItem? LastReceivedCluster { get; private set; } = null!;
        public async Task<AcknowledgedResponse> TransferDevice(string newDeviceId)
        {
            var connetState = await Library.ConnectState;
            var transfer = await connetState.Transfer(Library.Configuration.DeviceId, newDeviceId,
                new
                {
                    transfer_options = new
                    {
                        restore_paused = "restore"
                    }
                });
            return transfer;
        }
        public async Task<AcknowledgedResponse> InvokeCommandOnRemoteDevice(RemoteCommand playbackState,
            string deviceId = null)
        {
            var to = deviceId ?? ActiveDevice?.Id ?? Library.Configuration.DeviceId;
            var connetState = await Library.ConnectState;
            switch (playbackState)
            {
                case RemoteCommand.Pause:
                    return await connetState.Command(Library.Configuration.DeviceId, 
                        to, new
                    {
                        command = new
                        {
                            endpoint = "pause"
                        }
                    });
                case RemoteCommand.Play:
                    return await connetState.Command(Library.Configuration.DeviceId,
                        to, new
                    {
                        command = new
                        {
                            endpoint = "resume"
                        }
                    });
                    break;
                case RemoteCommand.Skip:
                    break;
                case RemoteCommand.Previous:
                    break;
                case RemoteCommand.ShuffleToggle:
                    break;
                case RemoteCommand.RepeatContext:
                    break;
                case RemoteCommand.RepeatTrack:
                    break;
                case RemoteCommand.RepeatOff:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playbackState), playbackState, null);
            }
            throw new ArgumentOutOfRangeException(nameof(playbackState), playbackState, null);
        }
        public async Task<AcknowledgedResponse> PlayItem(string connectClientCurrentDevice,
            IPlayRequest request)
        {
            var connetState = await Library.ConnectState;
            var to = connectClientCurrentDevice ?? ActiveDevice?.Id ?? Library.Configuration.DeviceId;

            return await connetState.Command(
                Library.Configuration.DeviceId,
                to, request.GetModel()); ;
        }

        public async Task<AcknowledgedResponse> Seek(double delta,
            string? deviceId = null)
        {
            var connetState = await Library.ConnectState;
            var to = deviceId ??
                     ActiveDevice?.Id ?? Library.Configuration.DeviceId;

            return await connetState.Command(Library.Configuration.DeviceId,
                to, new
                {
                    command = new
                    {
                        endpoint = "seek_to",
                        value = delta
                    }
                });
        }

        internal async void OnNewPlaybackWrapper(object sender, PlayerState state)
        {
            var newWrapper = await LocalStateToWrapper(null, state);
            LastReceivedCluster = newWrapper;

            NewItem?.Invoke(sender, (LastReceivedCluster, ActiveDevice));

            WaitForConnectionId.Set();
        }
        internal async void OnNewPlaybackWrapper(object sender, Cluster state)
        {
            var newWrapper = await LocalStateToWrapper(state);
            LastReceivedCluster = newWrapper;

            NewItem?.Invoke(sender, (LastReceivedCluster, ActiveDevice));

            WaitForConnectionId.Set();
        }

        internal void NotifyDeviceUpdates(object sender, Cluster cluster)
        {
            var devicesTemp = new List<IRemoteDevice>();
            foreach (var deviceInfo in cluster.Device)
            {
                //if (deviceInfo.Value.DeviceId == Library.Configuration.DeviceId)
                //{
                //    owndevice = DeviceInfoToRemoteDevice(deviceInfo.Value, cluster.ActiveDeviceId);
                //    continue;
                //}

                if (deviceInfo.Value.DeviceId == cluster.ActiveDeviceId)
                {
                    ActiveDevice = DeviceInfoToRemoteDevice(deviceInfo.Value, cluster.ActiveDeviceId);
                    continue;
                }

                devicesTemp.Add(DeviceInfoToRemoteDevice(deviceInfo.Value, cluster.ActiveDeviceId));
            }

            AllDevices = devicesTemp;
            WaitForConnectionId.Set();
        }
        internal void OnRepeatStateChanged(object sender, RepeatState state)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.RepeatState = state;
            }
            RepeatStateChanged?.Invoke(sender, state);
        }
        internal void OnPositionChanged(object sender, double pos)
        {
            PositionChanged?.Invoke(sender, pos);
        }

        internal void OnShuffleStatecHanged(object sender, bool isShuffling)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.IsShuffle = isShuffling;
            }
            IsShuffleCHanged?.Invoke(sender, isShuffling);
        }

        internal void OnPlaybackStateChanged(object sender, bool paused)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.IsPaused = paused;
            }
            IsPausedChanged?.Invoke(sender, paused);
        }
        private async Task<PlayingItem> LocalStateToWrapper(
       Cluster? cluster = null,
       PlayerState? playerState = null)
        {
            using (await clusterLock.LockAsync())
            {
                var currentState = cluster?.PlayerState ?? playerState;
                RepeatState repeatState;

                var repeatingTrack = currentState!.Options.RepeatingTrack;
                var repeatingContext = currentState!.Options.RepeatingContext;
                if (repeatingContext && !repeatingTrack)
                {
                    repeatState = RepeatState.Context;
                }
                else
                {
                    if (repeatingTrack)
                    {
                        repeatState = RepeatState.Track;
                    }
                    else
                    {
                        repeatState = RepeatState.Off;
                    }
                }

                var contextId = currentState.ContextUri?.UriToIdConverter();

                var itemId = currentState.Track.Uri?.UriToIdConverter();

                ITrackItem? item = null;
                var descriptions = new List<Descriptions>();
                var durationMs = 0;
                IAudioId? groupId = null;
                switch (itemId.AudioType)
                {
                    case AudioItemType.Track:
                        var tracksClient = await Library.TracksClient;
                        var fullTrack = await tracksClient.GetTrack(itemId.Id);
                        descriptions.AddRange(fullTrack.Artists.Select(z =>
                            new Descriptions(z.Name, new ArtistId((z as SimpleArtist).Uri))));
                        durationMs = fullTrack.DurationMs;
                        item = fullTrack;
                        groupId = fullTrack.Group.Id;
                        break;
                    case AudioItemType.Episode:
                        var episodesClient = await Library.EpisodesClient;
                        var fullEpisode = await episodesClient.GetEpisode(itemId.Id);
                        descriptions.AddRange(new Descriptions[]
                        {
                            new Descriptions(fullEpisode.Show.Name, fullEpisode.Show.Id)
                        });
                        durationMs = fullEpisode.DurationMs;
                        item = fullEpisode;
                        groupId = fullEpisode.Show.Id;
                        break;
                    default:
                        throw new NotImplementedException("?");
                }

               // var owndevice = default(IRemoteDevice);
                if (cluster != null)
                {
                    var devicesTemp = new List<IRemoteDevice>();
                    foreach (var deviceInfo in cluster.Device)
                    {
                        //if (deviceInfo.Value.DeviceId == Library.Configuration.DeviceId)
                        //{
                        //    owndevice = DeviceInfoToRemoteDevice(deviceInfo.Value, cluster.ActiveDeviceId);
                        //    continue;
                        //}

                        if (deviceInfo.Value.DeviceId == cluster.ActiveDeviceId)
                        {
                            ActiveDevice = DeviceInfoToRemoteDevice(deviceInfo.Value, cluster.ActiveDeviceId);
                            continue;
                        }

                        devicesTemp.Add(DeviceInfoToRemoteDevice(deviceInfo.Value, cluster.ActiveDeviceId));
                    }

                    AllDevices = devicesTemp;
                }

                var clustered = new PlayingItem(item, groupId, repeatState,
                    currentState.Options.ShufflingContext,
                    currentState.IsPaused,
                    ActiveDevice,
                    contextId,
                    currentState.Timestamp,
                    currentState.PositionAsOfTimestamp, descriptions
                    , TimeSpan.FromMilliseconds(durationMs),
                    AllDevices);
                LastReceivedCluster = clustered;
                return clustered;
            }
        }

        private IRemoteDevice DeviceInfoToRemoteDevice(DeviceInfo info,
            string activeDeviceId)
        {
            return new SpotifyDevice(info.DeviceId, info.Name,
                info.DeviceType.ToString(),
                !info.Capabilities.DisableVolume,
                (int)info.Volume,
                info.DeviceId == activeDeviceId);
        }
        public Task UpdateConnectionId(string header) => RequestState.UpdateConnectionId(header);

        public void OnNewCluster(Cluster tryGet)
        {
            MessageState.PreviousCluster = tryGet;
        }

        public void OnIncomingTransfer(object sender)
        {
            IncomingTransfer?.Invoke(sender, EventArgs.Empty);
        }

        public void OnTransferdone(object sender, Exception p1)
        {
            TransferDone?.Invoke(sender, p1);
        }
    }
}
