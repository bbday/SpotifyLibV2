using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Models.Public;
using SpotifyLibV2.Models.Request;

namespace SpotifyLibV2.Connect
{
    public class SpotifyConnectState : ISpotifyConnectState, IMessageListener
    {
        private readonly DeviceInfo _deviceInfo;
        private volatile string _connectionId = null;
        private readonly ISpotifyConnectReceiver _receiver;
        private readonly PutStateRequest _putState;
        private readonly SpotifyConfiguration _config;
        private long _previousSet;
        private bool? _previousPause;
        private bool? _previousShuffle;
        private RepeatState? _previousRepeatState;
        private PlayingChangedRequest _currentCluster;
        private readonly ITokensProvider _tokens;
        private HttpClient _putclient;
        public SpotifyConnectState(
            IDealerClient dealerClient,
            ISpotifyConnectReceiver receiver,
            ITokensProvider tokens,
            SpotifyConfiguration config,
            uint initialVolume,
            int volumeSteps)
        {
            _previousPause = null;
            _receiver = receiver;
            _config = config;
            _tokens = tokens;

            this._deviceInfo = InitializeDeviceInfo(initialVolume, volumeSteps);
            this._putState = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Connectstate.Device
                {
                    DeviceInfo = _deviceInfo
                }
            };
            this.ConnectState = InitState();
            dealerClient.AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
        }
        public string ActiveDeviceId { get; private set; }
        public PlayerState ConnectState { get; }
        public async Task UpdateState(PutStateReason reason, int playerTime, PlayerState state)
        {
            if (_connectionId == null) throw new ArgumentNullException(_connectionId);

            if (playerTime == -1) _putState.HasBeenPlayingForMs = 0L;
            else _putState.HasBeenPlayingForMs = (ulong)playerTime;

            _putState.PutStateReason = reason;
            _putState.ClientSideTimestamp = (ulong)TimeProvider.CurrentTimeMillis();
            _putState.Device.DeviceInfo = _deviceInfo;
            _putState.Device.PlayerState = state;
            await PutConnectState(_putState);
        }

        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            switch (uri)
            {
                case { } str when str.StartsWith("hm://pusher/v1/connections/"):
                    UpdateConnectionId(headers["Spotify-Connection-Id"]);
                    break;
                case { } str when str.Equals("hm://connect-state/v1/connect/volume"):
                    var cmd = SetVolumeCommand.Parser.ParseFrom(payload);
                    lock (this)
                    {
                        _deviceInfo.Volume = (uint)cmd.Volume;
                        if (cmd.CommandOptions != null)
                        {
                            _putState.LastCommandMessageId = ((uint)cmd.CommandOptions.MessageId);
                            _putState.LastCommandSentByDeviceId = string.Empty;
                        }
                    }

                    Debug.WriteLine("Update volume. volume: {0}/{1}", cmd.Volume, 100);
                    _receiver.VolumeChanged(cmd.Volume);
                    break;
                case { } str when str.StartsWith("hm://connect-state/v1/cluster"):
                    var update = ClusterUpdate.Parser.ParseFrom(payload);

                    if (_previousPause != update.Cluster.PlayerState.IsPaused)
                    {
                        _previousPause = update.Cluster.PlayerState.IsPaused;
                        _receiver.PauseChanged(update.Cluster.PlayerState.IsPaused);
                    }

                    var now = TimeProvider.CurrentTimeMillis();
                    Debug.WriteLine("Received cluster update at {0}: {1}", now, update);
                    var ts = update.Cluster.Timestamp - 3000; // Workaround

                    try
                    {
                        if (!_config.DeviceId.Equals(update.Cluster.ActiveDeviceId)
                            && _putState.IsActive
                            && (ulong) now > _putState.StartedPlayingAt
                            && (ulong) ts > _putState.StartedPlayingAt)
                        {

                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    var updated = update.Cluster.ActiveDeviceId != ActiveDeviceId;
                    if (updated)
                    {
                        ActiveDeviceId = update.Cluster.ActiveDeviceId;
                        _receiver.DeviceChanged(ActiveDeviceId);
                    }
                    long timeStamp = 0;
                    if (update?.Cluster?.PlayerState?.PositionAsOfTimestamp != _previousSet)
                    {
                        if (update.Cluster.PlayerState != null)
                        {
                            _previousSet = update.Cluster.PlayerState.PositionAsOfTimestamp;
                            var diff = (int) (TimeProvider.CurrentTimeMillis() - update.Cluster.PlayerState.Timestamp);
                            Debug.WriteLine("Expected timestamp: " +
                                            (int) (update.Cluster.PlayerState.PositionAsOfTimestamp + diff));
                            _receiver.PositionChanged(update.Cluster.PlayerState.PositionAsOfTimestamp + diff);
                            timeStamp = update.Cluster.PlayerState.PositionAsOfTimestamp + diff;
                        }
                    }

                    if (update.Cluster?.PlayerState?.Options != null)
                    {
                        if (update.Cluster.PlayerState.Options.ShufflingContext != _previousShuffle)
                        {
                            _receiver.ShuffleStateChanged(update.Cluster.PlayerState.Options.ShufflingContext);
                            _previousShuffle = update.Cluster.PlayerState.Options.ShufflingContext;
                        }

                        var repeatingTrack = update.Cluster.PlayerState.Options.RepeatingTrack;
                        var repeatingContext = update.Cluster.PlayerState.Options.RepeatingContext;
                        if (repeatingContext && !repeatingTrack)
                        {
                            _previousRepeatState = RepeatState.Context;
                            _receiver.RepeatStateChanged(RepeatState.Context);
                        }
                        else
                        {
                            if (repeatingTrack)
                            {
                                _previousRepeatState = RepeatState.Track;
                                _receiver.RepeatStateChanged(RepeatState.Track);
                            }
                            else
                            {
                                _previousRepeatState = RepeatState.Off;
                                _receiver.RepeatStateChanged(RepeatState.Off);
                            }
                        }
                    }

                    var j = new PlayingChangedRequest(
                        _previousRepeatState ?? RepeatState.Off, 
                        _previousShuffle ?? false,
                        update?.Cluster?.PlayerState?.Track?.Uri,
                        update?.Cluster.PlayerState.ContextUri,
                        update?.Cluster?.PlayerState?.IsPaused,
                        !update?.Cluster?.PlayerState?.IsPaused,
                        timeStamp
                    );
                    _currentCluster = j;
                    _receiver.NewItem(j);
                    break;
                case { }:
                    Debug.WriteLine("Message left unhandled! uri: {0}", uri);
                    break;
            }
            return Task.FromResult("done");
        }
        private void UpdateConnectionId([NotNull] string conId)
        {
            try
            {
                conId = HttpUtility.UrlDecode(conId, Encoding.UTF8);
            }
            catch (Exception)
            {
                //ignored
            }

            if (_connectionId != null && _connectionId.Equals(conId)) return;
            _connectionId = conId;
            Debug.WriteLine("Updated Spotify-Connection-Id: " + _connectionId);
            ConnectState.IsSystemInitiated = true;
            _ = UpdateState(PutStateReason.NewDevice, -1, ConnectState);
            _receiver.Ready();
        }
        private DeviceInfo InitializeDeviceInfo(uint initialVolume, int volumeSteps)
            => new()
            {
                CanPlay = true,
                Volume = initialVolume,
                Name = _config.DeviceName,
                DeviceId = _config.DeviceId,
                DeviceType = _config.DeviceType,
                DeviceSoftwareVersion = "Spotify-11.1.0",
                SpircVersion = "3.2.6",
                Capabilities = new Capabilities
                {
                    CanBePlayer = true,
                    GaiaEqConnectId = true,
                    SupportsLogout = true,
                    IsObservable = true,
                    CommandAcks = true,
                    SupportsRename = true,
                    SupportsTransferCommand = true,
                    SupportsCommandRequest = true,
                    VolumeSteps = volumeSteps,
                    SupportsGzipPushes = true,
                    NeedsFullPlayerState = true,
                    SupportedTypes =
                    {
                        new List<string>
                        {
                            {"audio/episode"},
                            {"audio/track"}
                        }
                    }
                }
            };

        private static PlayerState InitState()
        {
            return new()
            {
                PlaybackSpeed = 1.0,
                SessionId = string.Empty,
                PlaybackId = string.Empty,
                ContextRestrictions = new Restrictions(),
                Options = new ContextPlayerOptions
                {
                    RepeatingContext = false,
                    ShufflingContext = false,
                    RepeatingTrack = false
                },
                PositionAsOfTimestamp = 0,
                Position = 0,
                IsPlaying = false
            };
        }
        private async Task PutConnectState([NotNull] PutStateRequest req)
        {
            try
            {
                _putclient ??= new HttpClient
                {
                    BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
                };
                _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokens.GetToken("playlist-read").AccessToken);
                var byteArrayContent = new ByteArrayContent(req.ToByteArray());
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
                _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);
                var res = await _putclient.PutAsync($"/connect-state/v1/devices/{_config.DeviceId}", byteArrayContent);

                if (res.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Put new connect state:");
                }
                else
                {
                    Debugger.Break();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed updating state.", ex);
            }
        }
    }
}
