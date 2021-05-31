using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf.Collections;
using Nito.AsyncEx;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;
using Spotify.Lib.Models.Ids;
using Spotify.Lib.Models.Response;
using Spotify.Lib.Models.Response.SpotItems.SimpleItems;
using Spotify.Lib.Models.TracksnEpisodes;
using Websocket.Client;

namespace Spotify.Lib.Connect
{
    internal class SpotifyConnectReceiver : ISpotifyConnectReceiver
    {
        private readonly AsyncLock clusterLock = new AsyncLock();
        private ManualResetEvent _waitForConid = null!;
        private List<IRemoteDevice> _allDevices;
        private IRemoteDevice _activeDevice;

        private SpotifyMessageListener messageListener;
        private ISpotifyPlayer _playerInternal;

        internal SpotifyConnectReceiver()
        {
            Instance = this;
        }
        internal async Task<PlayingItem> Connect()
        {
            var dealerClient = new DealerClient();
            messageListener = new SpotifyMessageListener();
            dealerClient.Attach();
            var connected = await dealerClient.Connect();
            WaitForConnectionId.WaitOne();
            return LastReceivedCluster;
        }

        internal event EventHandler<(ISpotifyPlayer New, ISpotifyPlayer? Old)> PlayerChangedInternal;

        internal ISpotifyPlayer Player
        {
            get => _playerInternal;
            set
            {
                PlayerChangedInternal?.Invoke(this, (value, _playerInternal));
                _playerInternal = value;
            }
        }

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

        public bool IsPlayingOnRemoteDevice => ActiveDevice?.Id != SpotifyConfig.DeviceId;
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
        public static SpotifyConnectReceiver Instance { get; private set; }

        public async Task<AcknowledgedResponse> TransferDevice(string newDeviceId)
        {
            var connetState = await SpotifyClient.Instance.ConnectState;
            var transfer = await connetState.Transfer(SpotifyConfig.DeviceId, newDeviceId,
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
            var to = deviceId ?? ActiveDevice?.Id ?? SpotifyConfig.DeviceId;
            var connetState = await SpotifyClient.Instance.ConnectState;
            switch (playbackState)
            {
                case RemoteCommand.Pause:
                    return await connetState.Command(SpotifyConfig.DeviceId, 
                        to, new
                    {
                        command = new
                        {
                            endpoint = "pause"
                        }
                    });
                case RemoteCommand.Play:
                    return await connetState.Command(SpotifyConfig.DeviceId,
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
            var connetState = await SpotifyClient.Instance.ConnectState;
            var to = connectClientCurrentDevice ?? ActiveDevice?.Id ?? SpotifyConfig.DeviceId;

            return await connetState.Command(
                SpotifyConfig.DeviceId,
                to, request.GetModel()); ;
        }

        public void AttachPlayer(ISpotifyPlayer player)
        {
            try
            {
                player?.Dispose();
            }
            catch (Exception x)
            {

            }

            Player = player;
            PlayerChanged?.Invoke(this, player);
        }

        public async Task<AcknowledgedResponse> Seek(double delta,
            string? deviceId = null)
        {
            var connetState = await SpotifyClient.Instance.ConnectState;
            var to = deviceId ??
                     ActiveDevice?.Id ?? SpotifyConfig.DeviceId;

            return await connetState.Command(SpotifyConfig.DeviceId,
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
            if (state.PlayerState?.Track == null) return;
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

                //ITrackItem? item = null;
                //var descriptions = new List<Descriptions>();
                //var durationMs = 0;
                //ISpotifyId? groupId = null;
                //switch (itemId.AudioType)
                //{
                //    case AudioItemType.Track:
                //        var tracksClient = await SpotifyClient.Instance.TracksClient;
                //        var fullTrack = await tracksClient.GetTrack(itemId.Id);
                //        descriptions.AddRange(fullTrack.Artists.Select(z =>
                //            new Descriptions(z.Name, new ArtistId((z as SimpleArtist?).Value.Uri))));
                //        durationMs = fullTrack.DurationMs;
                //        item = fullTrack;
                //        groupId = fullTrack.Group.Id;
                //        break;
                //    case AudioItemType.Episode:
                //        var episodesClient = await SpotifyClient.Instance.EpisodesClient;
                //        var fullEpisode = await episodesClient.GetEpisode(itemId.Id);
                //        descriptions.AddRange(new Descriptions[]
                //        {
                //            new Descriptions(fullEpisode.Group.Name, fullEpisode.Group.Id)
                //        });
                //        durationMs = fullEpisode.DurationMs;
                //        item = fullEpisode;
                //        groupId = fullEpisode.Group.Id;
                //        break;
                //    default:
                //        throw new NotImplementedException("?");
                //}
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

                var clustered = new PlayingItem(itemId,
                    repeatState,
                    currentState.Options.ShufflingContext,
                    currentState.IsPaused,
                    ActiveDevice,
                    contextId,
                    currentState.Timestamp,
                    currentState.PositionAsOfTimestamp,
                    AllDevices,
                    currentState.NextTracks);
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

        public Task UpdateConnectionId(string header)
        {
            //return Task.CompletedTask;
            var conId = HttpUtility.UrlDecode(header, Encoding.UTF8);
            RequestListener = new SpotifyRequestListener(conId);
            return RequestListener.Initialize();
        }
        public SpotifyRequestListener RequestListener { get; private set; }
        public void OnNewCluster(Cluster tryGet)
        {
            //MessageState.PreviousCluster = tryGet;
        }

        public void OnIncomingTransfer(object sender)
        {
            IncomingTransfer?.Invoke(sender, EventArgs.Empty);
        }

        public void OnTransferdone(object sender, Exception p1)
        {
            TransferDone?.Invoke(sender, p1);
        }

        public event EventHandler<ISpotifyPlayer> PlayerChanged;
        public event EventHandler<RepeatedField<ProvidedTrack>> QueueUpdated;
        public void OnQueueUpdate(RepeatedField<ProvidedTrack> playerStateNextTracks)
        {
            QueueUpdated?.Invoke(null, playerStateNextTracks);
        }
    }
}
